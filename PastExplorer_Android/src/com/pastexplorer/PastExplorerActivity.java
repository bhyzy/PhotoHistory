package com.pastexplorer;

import pastexplorer.util.StackTraceUtil;

import com.pastexplorer.api.APIException;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;

public class PastExplorerActivity extends Activity implements OnClickListener {
	
	private Button _signInButton;
	private EditText _loginEditBox;
	private EditText _passwordEditBox;
	
	private static final String DEBUG_TAG = "PE Home";
	
    /** Called when the activity is first created. */
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.main);
        
        _signInButton = (Button)findViewById(R.id.signin);
        _signInButton.setOnClickListener(this);
        
        _loginEditBox = (EditText)findViewById(R.id.login);
        _passwordEditBox = (EditText)findViewById(R.id.password);
    }
    
    @Override
    protected void onResume() {
       super.onResume();
       
       if (User.isSignedIn()) {
    	   try {
			User.signOut();
		} catch (APIException e) {
			Log.e(DEBUG_TAG, StackTraceUtil.getStackTrace(e));
		}
       }
       _loginEditBox.setText("BH");
       _passwordEditBox.setText("qwe");
       _loginEditBox.requestFocus();
    }

	public void onClick(View v) {
		switch (v.getId()) {
		case R.id.signin:
			try {
				String userName = _loginEditBox.getText().toString();
				String password = _passwordEditBox.getText().toString();
				
				if (userName.isEmpty()) {
					Toast.makeText(this, getString(R.string.signInEmptyUsername), Toast.LENGTH_SHORT).show();
				}
				else if (password.isEmpty()) {
					Toast.makeText(this, getString(R.string.signInEmptyPassword), Toast.LENGTH_SHORT).show();
				}
				else {
					if (User.signIn(userName, password)) {
						startActivity(new Intent(this, DashboardActivity.class));
					}
					else {
						Toast.makeText(this, getString(R.string.signInFailed), Toast.LENGTH_SHORT).show();
					}
				}
			} catch (APIException e) {
				Log.e(DEBUG_TAG, StackTraceUtil.getStackTrace(e));
			}
			break;
		}
	}
    
    
}