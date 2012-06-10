package com.pastexplorer;

import java.util.ArrayList;

import pastexplorer.util.HttpUtil;
import pastexplorer.util.StackTraceUtil;
import android.app.AlertDialog;
import android.app.ListActivity;
import android.app.ProgressDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.graphics.Bitmap;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.ImageView;
import android.widget.TextView;

import com.pastexplorer.api.APIException;
import com.pastexplorer.api.AlbumData;
import com.pastexplorer.api.Client;
import com.pastexplorer.api.PhotoData;
import com.pastexplorer.api.UserData;

public class DashboardActivity extends ListActivity {

	private static final String DEBUG_TAG = "PE Dashboard";
	
	private ProgressDialog _progressDialog = null;
	private ArrayList<AlbumWithThumbnail> _albums = null;
	private AlbumAdapter _adapter;

	/** Called when the activity is first created. */
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.dashboard);

		_albums = new ArrayList<AlbumWithThumbnail>();
		_adapter = new AlbumAdapter(this, R.layout.album_item, _albums);
		setListAdapter(_adapter);
		
        Thread thread = new Thread(null, new Runnable() {
			public void run() {
				retrieveAlbums();	
			}
		}, "RetrieveAlbums");
        thread.start();
		_progressDialog = ProgressDialog.show(this,
				getString(R.string.pleaseWait),
				getString(R.string.retrievingAlbums), 
				true, true);
	}

	@Override
	public void onBackPressed() {
		AlertDialog.Builder builder = new AlertDialog.Builder(this);
		builder.setTitle(R.string.confirmSignOutTitle)
				.setMessage(R.string.confirmSignOutMessage)
				.setIcon(android.R.drawable.ic_dialog_alert)
				.setPositiveButton(android.R.string.yes,
						new DialogInterface.OnClickListener() {
							public void onClick(DialogInterface dialog,
									int which) {
								DashboardActivity.super.onBackPressed();
							}
						})
				.setNegativeButton(android.R.string.no, null)
				.show();
	}
	
	@Override
	protected void onListItemClick(android.widget.ListView l, View v, int position, long id) {
		 super.onListItemClick(l, v, position, id);
		 Intent albumIntent = new Intent(this, AlbumActivity.class);
		 albumIntent.putExtra("album_id", _albums.get(position).album.id);
		 startActivity(albumIntent);
	}
	
	private void retrieveAlbums() {
		
		try {
			_albums = new ArrayList<AlbumWithThumbnail>();
			
			Client client = User.createClient();
			UserData user = client.getUser(User.getUserName());
			
			for (int albumId : user.albums) {
				AlbumWithThumbnail albumItem = new AlbumWithThumbnail();
				albumItem.album = client.getAlbum(albumId);
				
				if (albumItem.album.photos.size() > 0) {
					PhotoData firstPhoto = client.getPhoto(albumItem.album.photos.get(0));
					if (firstPhoto.thumbnail != null && !firstPhoto.thumbnail.isEmpty()) {
						try {
							albumItem.thumbnail = HttpUtil.downloadImage(firstPhoto.thumbnail);
						}
						catch (Exception e) {
							Log.e(DEBUG_TAG, StackTraceUtil.getStackTrace(e));
						}
					}
				}
				
				_albums.add(albumItem);
			}
			//Thread.sleep(2000);
			
		} catch (APIException e) {
			Log.e(DEBUG_TAG, StackTraceUtil.getStackTrace(e));
		} catch (Exception e) {
			Log.e(DEBUG_TAG, StackTraceUtil.getStackTrace(e));
		}
		
		runOnUiThread(_updateAlbumList);
	}
	
	private Runnable _updateAlbumList = new Runnable() {
		public void run() {
			if (_albums != null && _albums.size() > 0) {
				_adapter.notifyDataSetChanged();
				for (AlbumWithThumbnail album : _albums)
					_adapter.add(album);
			}
			_progressDialog.dismiss();
			_adapter.notifyDataSetChanged();
		}
	};

	private class AlbumAdapter extends ArrayAdapter<AlbumWithThumbnail> {
		private ArrayList<AlbumWithThumbnail> _items;

		public AlbumAdapter(Context context, int textViewResourceId,
				ArrayList<AlbumWithThumbnail> items) {
			super(context, textViewResourceId, items);
			this._items = items;
		}

		@Override
		public View getView(int position, View convertView, ViewGroup parent) {
			View v = convertView;
			if (v == null) {
				LayoutInflater vi = (LayoutInflater) getSystemService(Context.LAYOUT_INFLATER_SERVICE);
				v = vi.inflate(R.layout.album_item, null);
			}
			AlbumWithThumbnail a = _items.get(position);
			if (a != null) {
				
				TextView nameLabel = (TextView) v.findViewById(R.id.name);
				TextView descriptionLabel = (TextView) v.findViewById(R.id.description);
				TextView photosLabel = (TextView) v.findViewById(R.id.noPhotos);
				TextView viewsLabel = (TextView) v.findViewById(R.id.views);
				TextView ratingLabel = (TextView) v.findViewById(R.id.rating);
				ImageView thumbnailImage = (ImageView) v.findViewById(R.id.thumbnail);
				
				if (nameLabel != null) {
					nameLabel.setText(a.album.name);
				}
				if (descriptionLabel != null) {
					descriptionLabel.setText(a.album.description);
				}
				if (photosLabel != null) {
					photosLabel.setText( getString(R.string.albumItemPhotos) + " " + a.album.photos.size() );
				}
				if (viewsLabel != null) {
					viewsLabel.setText( getString(R.string.albumItemViews) + " " + a.album.views );
				}
				if (ratingLabel != null) {
					ratingLabel.setText( getString(R.string.albumItemRating) + " " + a.album.rating );
				}
				if (thumbnailImage != null && a.thumbnail != null) {
					thumbnailImage.setImageBitmap(a.thumbnail);
				}
			}
			return v;
		}

	}
	
	private class AlbumWithThumbnail {
		public AlbumData album;
		public Bitmap thumbnail;
	}
}