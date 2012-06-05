package pastexplorer.util;

import java.io.IOException;
import java.io.InputStream;
import java.net.HttpURLConnection;
import java.net.URL;

import android.graphics.Bitmap;
import android.graphics.BitmapFactory;

import com.pastexplorer.api.Client;

public class HttpUtil {
	public static Bitmap downloadImage(String uri) throws IOException {		
		URL imageUri = new URL(uri);
		HttpURLConnection conn = (HttpURLConnection)imageUri.openConnection();
		conn.setUseCaches(false); 
		conn.setConnectTimeout(5000);
		conn.setAllowUserInteraction(false);
		conn.setRequestProperty("User-Agent", "PastExplorer Android Application");
		conn.setRequestProperty("Host", Client.API_SERVICE_HOST_HEADER);
		conn.setRequestProperty("Accept", "*/*");
		conn.connect();
		InputStream is = conn.getInputStream();
		return BitmapFactory.decodeStream(is);
	}
}