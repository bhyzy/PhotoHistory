package com.pastexplorer;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.os.Bundle;

public class DashboardActivity extends Activity {
    /** Called when the activity is first created. */
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.dashboard);
    }
    
    @Override
    public void onBackPressed() {  	
    	AlertDialog.Builder builder = new AlertDialog.Builder(this);
    	builder
    	.setTitle(R.string.confirmSignOutTitle)
    	.setMessage(R.string.confirmSignOutMessage)
    	.setIcon(android.R.drawable.ic_dialog_alert)
    	.setPositiveButton(android.R.string.yes, new DialogInterface.OnClickListener() {
    	    public void onClick(DialogInterface dialog, int which) {			      	
    	    	DashboardActivity.super.onBackPressed();
    	    }
    	})
    	.setNegativeButton(android.R.string.no, null)
    	.show();
    }

}