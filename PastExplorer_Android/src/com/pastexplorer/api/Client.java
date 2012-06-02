package com.pastexplorer.api;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.PrintStream;
import java.io.StringWriter;
import java.net.Authenticator;
import java.net.HttpURLConnection;
import java.net.PasswordAuthentication;
import java.net.URL;
import java.net.URLConnection;

import org.apache.http.HttpConnection;
import org.json.JSONException;
import org.json.JSONObject;

import android.util.Base64;
import android.util.Log;

public class Client {

	private String _userName;
	private String _password;
	
	//public static final String API_SERVICE_URI = "http://localhost:3518/api";
	public static final String API_SERVICE_HOST = "localhost:3518";
	public static final String API_SERVICE_URI = "http://10.0.2.2:3518/api";
	public static final String DEBUG_TAG = "PE Client";

	public Client(String userName, String password) {
		_userName = userName;
		_password = password;
	}
	
	public boolean verifyCredentials() throws APIException {
		try {
			String resultRaw = callService("users", "verify_credentials");
			JSONObject result = new JSONObject(resultRaw);
			boolean valid = result.getBoolean("ok");
			if (!valid) {
				Log.d(DEBUG_TAG, "verification error: " + result.getString("data"));
			}
			return valid;
		} 
		catch (Exception e) {
			throw new APIException("failed to verify user credentials", e);
		} 
	}

	public User getUser(String userName) throws APIException {
		throw new APIException("not implemented");
	}

	private String buildServiceQuery(String resource, String action, String id) {
		if (resource == null)
			throw new NullPointerException("resource");
		if (action == null)
			throw new NullPointerException("action");

		StringBuilder sb = new StringBuilder();

		sb.append(API_SERVICE_URI);
		sb.append("/");
		sb.append(resource);
		sb.append("/");
		sb.append(action);

		if (id != null) {
			sb.append("/");
			sb.append(id);
		}

		return sb.toString();
	}

	private String callService(String resource, String action) throws Exception {
		return callService(resource, action, null);
	}
	
	private String callService(String resource, String action, String id) throws Exception {

		// build query URL
		String address = buildServiceQuery(resource, action, id);
		Log.d(DEBUG_TAG, "callService URL: " + address);
		URL url = new URL(address);

		// make connection
		System.setProperty("http.keepAlive", "false");
		HttpURLConnection connection = (HttpURLConnection)url.openConnection();
		connection.setUseCaches(false); 
		connection.setConnectTimeout(5000);
		//connection.setDoOutput(true);
		connection.setAllowUserInteraction(false);
		//connection.setRequestProperty("User-Agent", "Mozilla/5.0 (compatible)");
		connection.setRequestProperty("Accept", "*/*");
		connection.setRequestProperty("Host", API_SERVICE_HOST);
		connection.setRequestProperty("Authorization",
				"Basic "+ Base64.encodeToString((_userName+":"+_password).getBytes(), Base64.DEFAULT));

		Log.d(DEBUG_TAG, "connecting...");
		connection.connect();
		Log.d(DEBUG_TAG, "response code: " + connection.getResponseCode());	
		
		try {
			if (connection.getResponseCode() != HttpURLConnection.HTTP_OK) {
				String errorMsg = convertStreamToString(connection.getErrorStream());
				if (connection.getResponseCode() == 500) {
					try {
						JSONObject errorObj = new JSONObject(errorMsg);
						throw new APIException(errorObj.getString("error"));
					}
					catch (JSONException e) {}
				}
				throw new APIException("service call resulted in response code " + connection.getResponseCode());
			}
		
			// send query
			//PrintStream ps = new PrintStream(connection.getOutputStream());
			//ps.print("");
			//ps.close();

			// retrieve result
			BufferedReader br = new BufferedReader(new InputStreamReader(
					connection.getInputStream(), "UTF-8"));
			StringBuilder sb = new StringBuilder();
			String line;
			while ((line = br.readLine()) != null) {
				sb.append(line);
				sb.append("\n");
			}
			br.close();
			
			String response = sb.toString();
			Log.d(DEBUG_TAG, "callService response: " + response);
			return response;
		}
		catch (Exception e) {
			throw e;
		}
		finally {
			connection.disconnect();
		}
	}

	private String convertStreamToString(InputStream is) throws Exception {
		BufferedReader reader = new BufferedReader(new InputStreamReader(is));
		StringBuilder sb = new StringBuilder();

		String line = null;
		try {
			while ((line = reader.readLine()) != null) {
				sb.append(line + "\n");
			}
		} finally {
			is.close();
		}
		return sb.toString();
	}

}