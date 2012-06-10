package com.pastexplorer;

import java.io.FileInputStream;
import java.io.IOException;
import java.util.Calendar;

import pastexplorer.util.StackTraceUtil;
import android.app.Activity;
import android.app.DatePickerDialog;
import android.app.Dialog;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.DatePicker;
import android.widget.ImageView;
import android.widget.TextView;

public class UploadPhotoActivity extends Activity {
	private static final String DEBUG_TAG = "PE Upload";
	
    private int mYear;
    private int mMonth;
    private int mDay;
    
    private TextView mDateDisplay;
    private Button mChangeDate;

    private static final int DATE_DIALOG_ID = 0;
	
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
        
        // get the current date
        final Calendar c = Calendar.getInstance();
        mYear = c.get(Calendar.YEAR);
        mMonth = c.get(Calendar.MONTH);
        mDay = c.get(Calendar.DAY_OF_MONTH);

        // display the current date (this method is below)
        updateDateDisplay();
        
        String photo = getIntent().getStringExtra("photo");
        if (photo != null) {
        	ImageView photoPreview = (ImageView)findViewById(R.id.photoPreview);
        	if (photoPreview != null) {
        		try {
					photoPreview.setImageBitmap( loadBitmapFromPrivateStorage(photo) );
				} catch (IOException e) {
					Log.e(DEBUG_TAG, "failed to load photo bitmap from file " + photo + ": " 
						+ StackTraceUtil.getStackTrace(e));
				}
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
}