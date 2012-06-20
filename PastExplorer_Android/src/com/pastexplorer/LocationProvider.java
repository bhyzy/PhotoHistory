package com.pastexplorer;

import android.content.Context;
import android.location.Location;
import android.location.LocationListener;
import android.location.LocationManager;
import android.os.Bundle;
import android.util.Log;

public class LocationProvider implements LocationListener {
	
	private static final String DEBUG_TAG = "PE GPS";
	
	private static LocationProvider mInstance = null;
	//private Context mContext;
	private Location mBestLocation = null;
	private LocationManager mLocationManager;
	private static final int FIX_VALIDITY_TIME_MS = 1000 * 60 * 2;
	
	public static void createInstance(Context context) {
		mInstance = new LocationProvider(context);
	}
	
	public static LocationProvider instance() {
		return mInstance;
	}
	
	private LocationProvider(Context context) {
		mLocationManager = (LocationManager) context.getSystemService(Context.LOCATION_SERVICE);
	}
	
	public void startListening() {
		mLocationManager.requestLocationUpdates(LocationManager.NETWORK_PROVIDER, 0, 0, this);
		mLocationManager.requestLocationUpdates(LocationManager.GPS_PROVIDER, 0, 0, this);
	}
	
	public void stopListening() {
		mLocationManager.removeUpdates(this);
	}
	
	public Location getBestLocation() {
		return mBestLocation;
	}

	public void onLocationChanged(Location location) {
		Log.d(DEBUG_TAG, "location changed: " + location.getLatitude() + "," + location.getLongitude());
		if (isBetterLocation(location, mBestLocation)) {
			Log.d(DEBUG_TAG, "new best location fix: " + location.getLatitude() + "," + location.getLongitude());
			mBestLocation = location;
		}
	}

	public void onProviderDisabled(String provider) {
		Log.d(DEBUG_TAG, "provider disabled: " + provider);
	}

	public void onProviderEnabled(String provider) {
		Log.d(DEBUG_TAG, "provider enabled: " + provider);
	}

	public void onStatusChanged(String provider, int status, Bundle extras) {
		Log.d(DEBUG_TAG, "provider status changed: " + provider + " => " + status);
	}
	
	protected boolean isBetterLocation(Location location, Location currentBestLocation) {
	    if (currentBestLocation == null) {
	        // A new location is always better than no location
	        return true;
	    }

	    // Check whether the new location fix is newer or older
	    long timeDelta = location.getTime() - currentBestLocation.getTime();
	    boolean isSignificantlyNewer = timeDelta > FIX_VALIDITY_TIME_MS;
	    boolean isSignificantlyOlder = timeDelta < -FIX_VALIDITY_TIME_MS;
	    boolean isNewer = timeDelta > 0;

	    // If it's been more than two minutes since the current location, use the new location
	    // because the user has likely moved
	    if (isSignificantlyNewer) {
	        return true;
	    // If the new location is more than two minutes older, it must be worse
	    } else if (isSignificantlyOlder) {
	        return false;
	    }

	    // Check whether the new location fix is more or less accurate
	    int accuracyDelta = (int) (location.getAccuracy() - currentBestLocation.getAccuracy());
	    boolean isLessAccurate = accuracyDelta > 0;
	    boolean isMoreAccurate = accuracyDelta < 0;
	    boolean isSignificantlyLessAccurate = accuracyDelta > 200;

	    // Check if the old and new location are from the same provider
	    boolean isFromSameProvider = isSameProvider(location.getProvider(),
	            currentBestLocation.getProvider());

	    // Determine location quality using a combination of timeliness and accuracy
	    if (isMoreAccurate) {
	        return true;
	    } else if (isNewer && !isLessAccurate) {
	        return true;
	    } else if (isNewer && !isSignificantlyLessAccurate && isFromSameProvider) {
	        return true;
	    }
	    return false;
	}

	/** Checks whether two providers are the same */
	private boolean isSameProvider(String provider1, String provider2) {
	    if (provider1 == null) {
	      return provider2 == null;
	    }
	    return provider1.equals(provider2);
	}
}
