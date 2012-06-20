package com.pastexplorer;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.util.Calendar;
import java.util.Date;

import pastexplorer.util.StackTraceUtil;
import pastexplorer.util.StorageUtil;
import android.app.Activity;
import android.app.AlertDialog;
import android.app.DatePickerDialog;
import android.app.Dialog;
import android.app.ProgressDialog;
import android.content.DialogInterface;
import android.content.Intent;
import android.graphics.Bitmap;
import android.os.Bundle;
import android.os.Environment;
import android.util.Log;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.DatePicker;
import android.widget.EditText;
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

	private int mAlbumId;
	private String mPhotoDescription;
	private Date mPhotoDate;
	
	private boolean mUploadSuccessful;
	private String mUploadError;
	
	/** Called when the activity is first created. */
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.upload_photo);

		mAlbumId = getIntent().getExtras().getInt("album_id");

		mDateDisplay = (TextView) findViewById(R.id.dateValue);
		mChangeDate = (Button) findViewById(R.id.dateChange);

		mChangeDate.setOnClickListener(new View.OnClickListener() {
			public void onClick(View v) {
				showDialog(DATE_DIALOG_ID);
			}
		});

		Button uploadButton = (Button) findViewById(R.id.upload);
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
		if (mPhotoFilename != null) {
			try {
				mPhoto = StorageUtil.loadBitmapFromPrivateStorage(this,
						mPhotoFilename);
			} catch (IOException e) {
				Log.e(DEBUG_TAG,
						"failed to load photo bitmap from file "
								+ mPhotoFilename + ": "
								+ StackTraceUtil.getStackTrace(e));
			}
			ImageView photoPreview = (ImageView) findViewById(R.id.photoPreview);
			if (photoPreview != null) {
				photoPreview.setImageBitmap(mPhoto);
			}
		}
	}

	@Override
	protected Dialog onCreateDialog(int id) {
		switch (id) {
		case DATE_DIALOG_ID:
			return new DatePickerDialog(this, mDateSetListener, mYear, mMonth,
					mDay);
		}
		return null;
	}

	// updates the date in the TextView
	private void updateDateDisplay() {
		mDateDisplay.setText(new StringBuilder().append(mMonth + 1).append("-")
				.append(mDay).append("-").append(mYear).append(" "));
	}

	private DatePickerDialog.OnDateSetListener mDateSetListener = new DatePickerDialog.OnDateSetListener() {
		public void onDateSet(DatePicker view, int year, int monthOfYear,
				int dayOfMonth) {
			mYear = year;
			mMonth = monthOfYear;
			mDay = dayOfMonth;
			updateDateDisplay();
		}
	};

	private void startUploadingPhoto() {
		mPhotoDescription = ((EditText) findViewById(R.id.descriptionValue))
				.getText().toString();
		mPhotoDate = new Date(mYear - 1900, mMonth, mDay);
		Log.d(DEBUG_TAG, "photo date: " + mPhotoDate.toString());

		mProgressDialog = ProgressDialog.show(this,
				getString(R.string.pleaseWait), "Uploading photo...", true,
				true);

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
			final int photoSize = (int) photoFile.length();
			Log.d(DEBUG_TAG, "photo size: " + photoSize);
			photoData = new byte[photoSize];
			FileInputStream photo = openFileInput(mPhotoFilename);
			photo.read(photoData);
			photo.close();
		} catch (IOException e) {
			Log.e(DEBUG_TAG, "failed to read photo pixels from file: "
					+ StackTraceUtil.getStackTrace(e));
			photoData = null;
		}

		if (photoData != null) {
			// upload photo
			try {
				Client client = User.createClient();
				Log.d(DEBUG_TAG, "uploading photo");
				client.uploadPhoto(photoData, mAlbumId, mPhotoDescription, mPhotoDate, LocationProvider.instance().getBestLocation());
				Log.d(DEBUG_TAG, "photo uploaded");
				mUploadSuccessful = true;
			} catch (APIException e) {
				Log.e(DEBUG_TAG, "failed to upload photo: " + StackTraceUtil.getStackTrace(e));
				mUploadSuccessful = false;
				mUploadError = e.getCause().getMessage();
			}
		}

		// finished:
		runOnUiThread(new Runnable() {
			public void run() {
				mProgressDialog.dismiss();
				
				AlertDialog alertDialog = new AlertDialog.Builder(UploadPhotoActivity.this).create();
				alertDialog.setTitle( mUploadSuccessful? getString(R.string.upload_ok) : getString(R.string.upload_error) );
				alertDialog.setMessage( mUploadSuccessful? getString(R.string.upload_ok_msg) : mUploadError );
				alertDialog.setButton(getString(android.R.string.ok), new DialogInterface.OnClickListener() {
				   public void onClick(DialogInterface dialog, int which) {
					   Intent intent = new Intent(UploadPhotoActivity.this, AlbumActivity.class);
					   intent.putExtra("album_id", mAlbumId);
					   startActivity(intent);
				   }
				});
				alertDialog.setIcon( mUploadSuccessful ? android.R.drawable.ic_dialog_info : android.R.drawable.ic_dialog_alert );
				alertDialog.setCancelable(false);
				alertDialog.show();
			}
		});
	}
}