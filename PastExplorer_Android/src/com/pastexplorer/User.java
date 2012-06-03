package com.pastexplorer;

import com.pastexplorer.api.APIException;
import com.pastexplorer.api.Client;

public class User {
	private static String _userName = null;
	private static String _password = null;
	
	public static boolean signIn(String userName, String password) throws APIException {
		Client client = new Client(userName, password);
		if (client.verifyCredentials()) {
			_userName = userName;
			_password = password;
			return true;
		}
		return false;
	}
	public static void signOut() throws APIException {
		if (!isSignedIn()) {
			throw new APIException("tried to sign out without signing in earlier");
		}
		_userName = null;
		_password = null;
	}
	public static boolean isSignedIn() {
		return _userName != null && _password != null;
	}
	
	public static Client createClient() throws APIException {
		if (!isSignedIn()) {
			throw new APIException("tried to instantiate Client instance without signing in");
		}
		return new Client(_userName, _password);
	}
}