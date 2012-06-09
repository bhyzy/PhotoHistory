package com.pastexplorer.api;

import java.io.BufferedReader;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.URL;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import android.util.Base64;
import android.util.Log;

public class Client {

	private String _userName;
	private String _password;
	
	public static final String API_SERVICE_HOST_HEADER = "localhost:3518";
	public static final String API_SERVICE_HOST_REAL = "192.168.1.13:5000";
	public static final String API_SERVICE_URI = "http://" + API_SERVICE_HOST_REAL + "/api";
	public static final String DEBUG_TAG = "PE Client";

	public Client(String userName, String password) {
		_userName = userName;
		_password = password;
	}
	
	public boolean verifyCredentials() throws APIException {
		try {
			String resultRaw = callService("users", "verify_credentials", null);
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

	public UserData getUser(String userName) throws APIException {
		try {
			String resultRaw = callService("users", null, userName);
			Log.d(DEBUG_TAG, "getUser raw result: " + resultRaw);
			JSONObject result = new JSONObject(resultRaw);
			if (!result.getBoolean("ok")) {
				throw new Exception(result.getString("error"));
			}
			
			JSONObject userRaw = result.getJSONObject("data");
			UserData user = new UserData();
			
			user.id = userRaw.getInt("id");
			user.userName = userRaw.getString("username");
			user.dateOfBirth = convertJSONObjectToDate(userRaw.getJSONObject("date_of_birth"));
			user.about = userRaw.getString("about");
			user.albums = convertJSONArrayToResourceIDs(userRaw.getJSONArray("albums"));
			
			return user;
		}
		catch (Exception e) {
			throw new APIException("failed to get user '" + userName + "'", e);
		}
	}
	
	public AlbumData getAlbum(int id) throws APIException {
		try {
			String resultRaw = callService("albums", null, String.valueOf(id));
			Log.d(DEBUG_TAG, "getAlbum raw result: " + resultRaw);
			JSONObject result = new JSONObject(resultRaw);
			if (!result.getBoolean("ok")) {
				throw new Exception(result.getString("error"));
			}
			
			JSONObject albumRaw = result.getJSONObject("data");
			AlbumData album = new AlbumData();
			
			album.id = albumRaw.getInt("id");
			album.name = albumRaw.getString("name");
			album.description = albumRaw.getString("description");
			album.category = albumRaw.getString("category");
			album.isPublic = albumRaw.getBoolean("is_public");
			album.rating = albumRaw.getInt("rating");
			album.views = albumRaw.getInt("views");
			album.photos = convertJSONArrayToResourceIDs(albumRaw.getJSONArray("photos"));
			
			return album;
		}
		catch (Exception e) {
			throw new APIException("failed to get album '" + id + "'", e);
		}		
	}
	
	public PhotoData getPhoto(int id) throws APIException {
		try {
			String resultRaw = callService("photos", null, String.valueOf(id));
			Log.d(DEBUG_TAG, "getPhoto raw result: " + resultRaw);
			JSONObject result = new JSONObject(resultRaw);
			if (!result.getBoolean("ok")) {
				throw new Exception(result.getString("error"));
			}
			
			JSONObject photoRaw = result.getJSONObject("data");
			PhotoData photo = new PhotoData();
			
			photo.id = photoRaw.getInt("id");
			photo.album = extractResourceID(photoRaw.getString("album"));
			photo.date = convertJSONObjectToDate(photoRaw.getJSONObject("date"));
			photo.description = photoRaw.getString("description");
			photo.image = photoRaw.getString("image").replace(API_SERVICE_HOST_HEADER, API_SERVICE_HOST_REAL);
			photo.thumbnail = photoRaw.getString("thumbnail").replace(API_SERVICE_HOST_HEADER, API_SERVICE_HOST_REAL);
			photo.latitude = photoRaw.isNull("latitude") == false? photoRaw.getDouble("latitude") : null;
			photo.longitude = photoRaw.isNull("longitude") == false? photoRaw.getDouble("longitude") : null;
			
			return photo;
		}
		catch (Exception e) {
			throw new APIException("failed to get photo '" + id + "'", e);
		}	
	}

	private int extractResourceID(String resourceUri) throws JSONException {
		return Integer.parseInt( resourceUri.substring(resourceUri.lastIndexOf('/') + 1) );
	}

	private Date convertJSONObjectToDate(JSONObject json) throws JSONException {
		return new Date( json.getInt("year"), json.getInt("month"), json.getInt("day") );
	}
	
	private List<Integer> convertJSONArrayToResourceIDs(JSONArray array) throws JSONException {
		List<Integer> ids = new ArrayList<Integer>();
		for (int i = 0; i < array.length(); i++) {
			ids.add( extractResourceID(array.getString(i)) );
		}
		return ids;
	}

	private String buildServiceQuery(String resource, String action, String id) throws Exception {
		if (resource == null)
			throw new NullPointerException("resource is missing");
		if (action == null && id == null)
			throw new Exception("either action or id has to be specified");

		StringBuilder sb = new StringBuilder();
		sb.append(API_SERVICE_URI);
		
		sb.append("/");
		sb.append(resource);
		
		if (action != null) {
			sb.append("/");
			sb.append(action);	
		}

		if (id != null) {
			sb.append("/");
			sb.append(id);
		}

		return sb.toString();
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
		connection.setRequestProperty("User-Agent", "PastExplorer Android Application");
		connection.setRequestProperty("Accept", "*/*");
		connection.setRequestProperty("Host", API_SERVICE_HOST_HEADER);
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