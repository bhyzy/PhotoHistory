package com.pastexplorer;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.nio.ByteBuffer;
import java.util.Calendar;
import java.util.Date;

import pastexplorer.util.StackTraceUtil;
import android.app.Activity;
import android.app.DatePickerDialog;
import android.app.Dialog;
import android.app.ProgressDialog;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.os.Bundle;
import android.os.Environment;
import android.util.Log;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.DatePicker;
import android.widget.ImageView;
import android.widget.TextView;

import com.pastexplorer.api.APIException;
import com.pastexplorer.api.Client;

public class UploadPhotoActivity extends Activity {
	private static final String DEBUG_TAG = "PE Upload";
	
    private int mYear;
    private int mMonth;
    private int mDay;
    
    private TextView mDateDisplay;
    private Button mChangeDate;
    private ProgressDialog mProgressDialog;
    
    private static final int DATE_DIALOG_ID = 0;
    
    private Bitmap mPhoto = null;
    private String mPhotoFilename = null;
	
    /** Called when the activity is first created. */
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.upload_photo);
        
        mDateDisplay = (TextView) findViewById(R.id.dateValue);
        mChangeDate = (Button) findViewById(R.id.dateChange);

        mChangeDate.setOnClickListener(new View.OnClickListener() {
            public void onClick(View v) {
                showDialog(DATE_DIALOG_ID);
            }
        });
        
        Button uploadButton = (Button)findViewById(R.id.upload);
        uploadButton.setOnClickListener(new OnClickListener() {
			public void onClick(View v) {
				startUploadingPhoto();
			}
		});
        
        // get the current date
        final Calendar c = Calendar.getInstance();
        mYear = c.get(Calendar.YEAR);
        mMonth = c.get(Calendar.MONTH);
        mDay = c.get(Calendar.DAY_OF_MONTH);

        // display the current date (this method is below)
        updateDateDisplay();
        
        // read photo thumbnail bitmap from file (passed by TakePhoto activity)
        mPhotoFilename = getIntent().getStringExtra("photo");
        //mPhotoFilename = ""; // TODO: tmp
        if (mPhotoFilename != null) {
        	//mPhoto = BitmapFactory.decodeResource(getResources(), R.drawable.photo); // TODO: tmp
    		try {
				mPhoto = loadBitmapFromPrivateStorage(mPhotoFilename);
			} catch (IOException e) {
				Log.e(DEBUG_TAG, "failed to load photo bitmap from file " + mPhotoFilename + ": " 
					+ StackTraceUtil.getStackTrace(e));
			}
        	ImageView photoPreview = (ImageView)findViewById(R.id.photoPreview);
        	if (photoPreview != null) {
        		photoPreview.setImageBitmap(mPhoto);
        	}
        }
    }
    
    @Override
    protected Dialog onCreateDialog(int id) {
        switch (id) {
        case DATE_DIALOG_ID:
            return new DatePickerDialog(this, mDateSetListener, mYear, mMonth, mDay);
        }
        return null;
    }
    
    // updates the date in the TextView
    private void updateDateDisplay() {
        mDateDisplay.setText(
            new StringBuilder()
                    .append(mMonth + 1).append("-")
                    .append(mDay).append("-")
                    .append(mYear).append(" "));
    }
    
	private DatePickerDialog.OnDateSetListener mDateSetListener = 
		new DatePickerDialog.OnDateSetListener() {
			public void onDateSet(DatePicker view, int year, int monthOfYear, int dayOfMonth) {
				mYear = year;
				mMonth = monthOfYear;
				mDay = dayOfMonth;
				updateDateDisplay();
			}
		};
		
	private Bitmap loadBitmapFromPrivateStorage(String filename) throws IOException {
		FileInputStream fis = openFileInput(filename);
		Bitmap bitmap = BitmapFactory.decodeStream(fis);
		fis.close();
		return bitmap;
	}
	
	private void startUploadingPhoto() {
		mProgressDialog = ProgressDialog.show(this,
				getString(R.string.pleaseWait),
				"Uploading photo...", 
				true, true);
		new Thread(new Runnable() {
			public void run() {
				uploadPhoto();
			}
		}).start();
	}
	
	private void uploadPhoto() {
	
		// read photo into byte[] array
		byte[] photoData = null;
		try {
		    File photoFile = new File(getFilesDir(), "tmp");
		    final int photoSize = (int)photoFile.length();
		    Log.d(DEBUG_TAG, "photo size: " + photoSize);
		    photoData = new byte[photoSize];
			FileInputStream photo = openFileInput(mPhotoFilename);
			photo.read(photoData);
			photo.close();	
		} catch (IOException e) {
			Log.e(DEBUG_TAG, "failed to read photo pixels from file: " + StackTraceUtil.getStackTrace(e));
			photoData = null;
		}
		
//		final int bufferSize = mPhoto.getRowBytes() * mPhoto.getHeight();
//		Log.d(DEBUG_TAG, "allocating buffer of size " + bufferSize);
//		ByteBuffer buffer = ByteBuffer.allocate(bufferSize);
//		Log.d(DEBUG_TAG, "allocated, copying...");
//		mPhoto.copyPixelsToBuffer(buffer);
//		Log.d(DEBUG_TAG, "copied");
//		photoData = buffer.array();
		
//    	
	      File photo= new File(Environment.getExternalStorageDirectory(), "test.jpg");

	      if (photo.exists()) {
	    	  Log.d(DEBUG_TAG, "Deleting existing photo");
	        photo.delete();
	      }

	      try {
	        FileOutputStream fos=new FileOutputStream(photo.getPath());
	        fos.write(photoData);
	        fos.close();
	        Log.d(DEBUG_TAG, "Photo saved");
	      }
	      catch (Exception e) {
	    	  e.printStackTrace();
	      }
		
		if (photoData != null) {
			// upload photo
			try {
				User.signIn("BH", "qwe");
				Client client = User.createClient();
				//Client client = new Client("BH", "qwe");
				Log.d(DEBUG_TAG, "uploading photo");
				client.uploadPhoto(photoData, 15, "test", new Date());
				Log.d(DEBUG_TAG, "photo uploaded");
			} catch (APIException e) {
				Log.e(DEBUG_TAG, "failed to upload photo: " + StackTraceUtil.getStackTrace(e));
			}			
		}

		// finished:
		runOnUiThread(new Runnable() {
			public void run() {
				mProgressDialog.dismiss();
				//Intent intent = new Intent(this, AlbumActivity.class);
			}
		});
	}
}