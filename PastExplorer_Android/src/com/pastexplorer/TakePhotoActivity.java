package com.pastexplorer;

import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;

import pastexplorer.util.StackTraceUtil;
import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.content.pm.ActivityInfo;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.drawable.BitmapDrawable;
import android.hardware.Camera;
import android.hardware.Camera.CameraInfo;
import android.os.Bundle;
import android.os.PowerManager;
import android.util.Log;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.Window;
import android.view.WindowManager;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.Toast;

public class TakePhotoActivity extends Activity implements OnClickListener {	
	private static final String DEBUG_TAG = "PE TakePhoto";
	
	private PowerManager.WakeLock mWakeLock;
	
   private PhotoPreview mPreview;
    Camera mCamera;
    int numberOfCameras;
    int cameraCurrentlyLocked;

    // The first rear facing camera
    int defaultCameraId;
    
    private Button mTakePhotoButton = null;
    private Button mConfirmPhotoButton = null;
    private Button mRetakePhotoButton = null;
    private ImageView mImage = null;
    private Bitmap mTakenPhoto = null;
    
    private boolean mTakingPhoto;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        // Hide the window title.
        requestWindowFeature(Window.FEATURE_NO_TITLE);
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN);

        setContentView(R.layout.take_photo);
        mPreview = (PhotoPreview)findViewById(R.id.preview);
        
        mImage = (ImageView)findViewById(R.id.photo);
        
        mTakePhotoButton = (Button)findViewById(R.id.take_photo);
        if (mTakePhotoButton != null) {
        	mTakePhotoButton.setOnClickListener(this);
        }
        mConfirmPhotoButton = (Button)findViewById(R.id.confirm_photo);
        if (mConfirmPhotoButton != null) {
        	mConfirmPhotoButton.setOnClickListener(this);
        }
        mRetakePhotoButton = (Button)findViewById(R.id.retake_photo);
        if (mRetakePhotoButton != null) {
        	mRetakePhotoButton.setOnClickListener(this);
        }

        // Find the total number of cameras available
        numberOfCameras = Camera.getNumberOfCameras();
        Log.d(DEBUG_TAG, "numberOfCameras = " + numberOfCameras);

        // Find the ID of the default camera
        CameraInfo cameraInfo = new CameraInfo();
        for (int i = 0; i < numberOfCameras; i++) {
            Camera.getCameraInfo(i, cameraInfo);
            if (cameraInfo.facing == CameraInfo.CAMERA_FACING_BACK) {
                defaultCameraId = i;
                Log.d(DEBUG_TAG, "defaultCameraId = " + defaultCameraId);
                break;
            }
        }
        
        PowerManager pm = (PowerManager) getSystemService(Context.POWER_SERVICE);
        mWakeLock = pm.newWakeLock(PowerManager.FULL_WAKE_LOCK, "DoNotDimScreen");
        
        mTakingPhoto = true;
    }

    @Override
    protected void onResume() {
        super.onResume();
        
        // Open the default i.e. the first rear facing camera.
 		mCamera = null;
 		try {
 	        mCamera = Camera.open();
 		}
 		catch (Exception e) {
 			Log.e(DEBUG_TAG, "failed to acquire camera object");
 			Toast.makeText(this, "Failed to acquire camera", Toast.LENGTH_SHORT);
 		}
 		cameraCurrentlyLocked = defaultCameraId;
        mPreview.setCamera(mCamera);
        
        setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_LANDSCAPE);
        if (mTakingPhoto) { 
        	prepareToTakePhoto();
        }
    }

    @Override
    protected void onPause() {
        super.onPause();
		
        // Because the Camera object is a shared resource, it's very
        // important to release it when the activity is paused.
        if (mCamera != null) {
            mPreview.setCamera(null);
            mCamera.release();
            mCamera = null;
        }
        
        setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_UNSPECIFIED);
        if (mTakingPhoto) {
        	finalizeTakingPhoto();
        }
    }

	@Override
	public void onClick(View v) {
		switch (v.getId()) {
		case R.id.take_photo:
			takePhoto();
			break;
		case R.id.confirm_photo:
			Intent intent = new Intent(this, UploadPhotoActivity.class);
			try {
				intent.putExtra("photo", saveBitmapToPrivateStorage(mTakenPhoto));
			} catch (IOException e) {
				Log.e(DEBUG_TAG, "failed to save photo bitmap to private internal storage: " 
						+ StackTraceUtil.getStackTrace(e));
			}
			startActivity(intent);
			break;
		case R.id.retake_photo:
			prepareToTakePhoto();
			mTakingPhoto = true;
			break;
		}
	};
	
	private String saveBitmapToPrivateStorage(Bitmap bitmap) throws IOException {
		final String FILENAME = "tmp";
		FileOutputStream fos = openFileOutput(FILENAME, Context.MODE_PRIVATE);
		bitmap.compress(Bitmap.CompressFormat.JPEG, 80, fos);
		fos.flush();
		fos.close();
		return FILENAME;
	}
	
	private void prepareToTakePhoto() {
		mTakePhotoButton.setVisibility(View.VISIBLE);
		mConfirmPhotoButton.setVisibility(View.GONE);
		mRetakePhotoButton.setVisibility(View.GONE);
		
		if (mImage != null) {
			// TODO: set last photo overlay
			mImage.setImageResource(R.drawable.photo);
	    	mImage.setAlpha(130);
		}

        mWakeLock.acquire();
        
        mCamera.startPreview();
	}
	
	private void finalizeTakingPhoto() {
		mTakePhotoButton.setVisibility(View.GONE);
		mConfirmPhotoButton.setVisibility(View.VISIBLE);
		mRetakePhotoButton.setVisibility(View.VISIBLE);
        
		mWakeLock.release();
	}
	
	private void takePhoto() {
		mCamera.takePicture(null, null, new Camera.PictureCallback() {
			@Override
			public void onPictureTaken(byte[] data, Camera camera) {
				//new SavePhotoTask().execute(data);
				
				try {
					Log.d(DEBUG_TAG, "Converting captured photo to a bitmap...");
					mTakenPhoto = BitmapFactory.decodeByteArray(data, 0, data.length);
					Log.d(DEBUG_TAG, "Successfully converted");
					mImage.setImageDrawable(new BitmapDrawable(mTakenPhoto));
					mImage.setAlpha(255);
				}
				catch (Exception e) {
					Log.e(DEBUG_TAG, "failed to decode captured photo data into a bitmap: " 
							+ StackTraceUtil.getStackTrace(e));
				}
				
				finalizeTakingPhoto();
				mTakingPhoto = false;
			}
		});	
	}
	
//	  private class SavePhotoTask extends AsyncTask<byte[], String, String> {
//	    @Override
//	    protected String doInBackground(byte[]... jpeg) {
//	    	Log.d(DEBUG_TAG, "Storing photo...");
//	    	
//	      File photo=
//	          new File(Environment.getExternalStorageDirectory(), "photo.jpg");
//
//	      if (photo.exists()) {
//	    	  Log.d(DEBUG_TAG, "Deleting existing photo");
//	        photo.delete();
//	      }
//
//	      try {
//	        FileOutputStream fos=new FileOutputStream(photo.getPath());
//
//	        fos.write(jpeg[0]);
//	        fos.close();
//	        Log.d(DEBUG_TAG, "Photo saved");
//	      }
//	      catch (java.io.IOException e) {
//	        Log.e(DEBUG_TAG, "Exception in photoCallback", e);
//	      }
//
//	      return(null);
//	    }
//	  }

//    @Override
//    public boolean onCreateOptionsMenu(Menu menu) {
//
//        // Inflate our menu which can gather user input for switching camera
//        MenuInflater inflater = getMenuInflater();
//        inflater.inflate(R.menu.camera_menu, menu);
//        return true;
//    }
//
//    @Override
//    public boolean onOptionsItemSelected(MenuItem item) {
//        // Handle item selection
//        switch (item.getItemId()) {
//        case R.id.switch_cam:
//            // check for availability of multiple cameras
//            if (numberOfCameras == 1) {
//                AlertDialog.Builder builder = new AlertDialog.Builder(this);
//                builder.setMessage(this.getString(R.string.camera_alert))
//                       .setNeutralButton("Close", null);
//                AlertDialog alert = builder.create();
//                alert.show();
//                return true;
//            }
//
//            // OK, we have multiple cameras.
//            // Release this camera -> cameraCurrentlyLocked
//            if (mCamera != null) {
//                mCamera.stopPreview();
//                mPreview.setCamera(null);
//                mCamera.release();
//                mCamera = null;
//            }
//
//            // Acquire the next camera and request Preview to reconfigure
//            // parameters.
//            mCamera = Camera
//                    .open((cameraCurrentlyLocked + 1) % numberOfCameras);
//            cameraCurrentlyLocked = (cameraCurrentlyLocked + 1)
//                    % numberOfCameras;
//            mPreview.switchCamera(mCamera);
//
//            // Start the preview
//            mCamera.startPreview();
//            return true;
//        default:
//            return super.onOptionsItemSelected(item);
//        }
//    }
	

}