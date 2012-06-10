package com.pastexplorer;

import java.util.ArrayList;

import pastexplorer.util.HttpUtil;
import pastexplorer.util.StackTraceUtil;
import android.app.Activity;
import android.app.ProgressDialog;
import android.content.Context;
import android.content.Intent;
import android.graphics.Bitmap;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.view.ViewGroup;
import android.view.View.OnClickListener;
import android.widget.BaseAdapter;
import android.widget.Button;
import android.widget.GridView;
import android.widget.ImageView;
import android.widget.TextView;

import com.pastexplorer.api.APIException;
import com.pastexplorer.api.AlbumData;
import com.pastexplorer.api.Client;
import com.pastexplorer.api.PhotoData;

public class AlbumActivity extends Activity implements OnClickListener {
	
	private static final String DEBUG_TAG = "PE Album";
	
	private int _albumID;
	private AlbumData _albumInfo = null;
	private ProgressDialog _progressDialog = null;
	private ArrayList<PhotoWithThumbnail> _photos = null;
	private PhotoAdapter _adapter = null;
	
    /** Called when the activity is first created. */
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.album);
        
       Button addPhotoBtn = (Button)findViewById(R.id.add_photo);
       addPhotoBtn.setOnClickListener(this);
        
    	Bundle extras = getIntent().getExtras();
    	_albumID = extras.getInt("album_id");
        
    	_photos = new ArrayList<PhotoWithThumbnail>();
    	_adapter = new PhotoAdapter(this, _photos);
		GridView photosGridView = (GridView) findViewById(R.id.photos);
		photosGridView.setAdapter(_adapter);
		
        Thread thread = new Thread(null, new Runnable() {
			public void run() {
				retrievePhotos();	
			}
		}, "RetrievePhotos");
        thread.start();
		_progressDialog = ProgressDialog.show(this,
				getString(R.string.pleaseWait),
				getString(R.string.retrievingPhotos), 
				true, true);
    }
    
    public void onClick(View v) {
    	switch (v.getId()) {
    	case R.id.add_photo:
    		Log.d(DEBUG_TAG, "onClick add_photo");
    		Intent intent = new Intent(this, TakePhotoActivity.class);
    		startActivity(intent);
    		break;
    	}
    }
    
	private void retrievePhotos() {
		
		try {
			_photos.clear();
			
			Client client = User.createClient();
			_albumInfo = client.getAlbum(_albumID);
			
			for (int photoId : _albumInfo.photos) {
				PhotoWithThumbnail photoItem = new PhotoWithThumbnail();
				photoItem.photo = client.getPhoto(photoId);
				if (photoItem.photo.thumbnail != null && !photoItem.photo.thumbnail.isEmpty()) {
					try {
						photoItem.thumbnail = HttpUtil.downloadImage(photoItem.photo.thumbnail);
					}
					catch (Exception e) {
						Log.e(DEBUG_TAG, StackTraceUtil.getStackTrace(e));
					}
				}
				
				_photos.add(photoItem);
			}
			//Thread.sleep(2000);
			
		} catch (APIException e) {
			Log.e(DEBUG_TAG, StackTraceUtil.getStackTrace(e));
		} catch (Exception e) {
			Log.e(DEBUG_TAG, StackTraceUtil.getStackTrace(e));
		}
		
		runOnUiThread(_updateAlbumContent);
	}
	
	private Runnable _updateAlbumContent = new Runnable() {
		public void run() {
			TextView nameLabel = (TextView)findViewById(R.id.name);
			TextView descriptionLabel = (TextView)findViewById(R.id.description);
			TextView photosLabel = (TextView)findViewById(R.id.noPhotos);
			TextView viewsLabel = (TextView)findViewById(R.id.views);
			TextView ratingLabel = (TextView)findViewById(R.id.rating);
			
			if (nameLabel != null) {
				nameLabel.setText(_albumInfo.name);
			}
			if (descriptionLabel != null) {
				descriptionLabel.setText(_albumInfo.description);
			}
			if (photosLabel != null) {
				photosLabel.setText( getString(R.string.albumItemPhotos) + " " + _albumInfo.photos.size() );
			}
			if (viewsLabel != null) {
				viewsLabel.setText( getString(R.string.albumItemViews) + " " + _albumInfo.views );
			}
			if (ratingLabel != null) {
				ratingLabel.setText( getString(R.string.albumItemRating) + " " + _albumInfo.rating );
			}
			
			_adapter.notifyDataSetChanged();
			_progressDialog.dismiss();
		}
	};
    
    public class PhotoAdapter extends BaseAdapter {
        private Context _context;
        private ArrayList<PhotoWithThumbnail> _items;

        public PhotoAdapter(Context c, ArrayList<PhotoWithThumbnail> items) {
            this._context = c;
            this._items = items;
        }

        public int getCount() {
            return _items.size();
        }

        public Object getItem(int position) {
            return _items.get(position);
        }

        public long getItemId(int position) {
            return _items.get(position).photo.id;
        }

        public View getView(int position, View convertView, ViewGroup parent) {
            ImageView imageView;
            if (convertView == null) {
                imageView = new ImageView(_context);
                //imageView.setLayoutParams(new GridView.LayoutParams(85, 85));
                imageView.setScaleType(ImageView.ScaleType.FIT_CENTER);
                //imageView.setPadding(8, 8, 8, 8);
            } else {
                imageView = (ImageView) convertView;
            }

            imageView.setImageBitmap(_items.get(position).thumbnail);
            return imageView;
        }
    }
    
	private class PhotoWithThumbnail {
		public PhotoData photo;
		public Bitmap thumbnail;
	}
}