package pastexplorer.util;

import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;

public final class StorageUtil {
	
	public static String saveBitmapToPrivateStorage(Context context, Bitmap bitmap) throws IOException {
		final String FILENAME = "tmp";
		FileOutputStream fos = context.openFileOutput(FILENAME, Context.MODE_PRIVATE);
		bitmap.compress(Bitmap.CompressFormat.JPEG, 80, fos);
		fos.flush();
		fos.close();
		return FILENAME;
	}
	
	public static Bitmap loadBitmapFromPrivateStorage(Context context, String filename) throws IOException {
		FileInputStream fis = context.openFileInput(filename);
		Bitmap bitmap = BitmapFactory.decodeStream(fis);
		fis.close();
		return bitmap;
	}
}