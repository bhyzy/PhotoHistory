package com.pastexplorer;

import java.util.ArrayList;

import android.app.AlertDialog;
import android.app.ListActivity;
import android.app.ProgressDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.TextView;

import com.pastexplorer.api.APIException;
import com.pastexplorer.api.AlbumData;
import com.pastexplorer.api.Client;
import com.pastexplorer.api.UserData;

public class DashboardActivity extends ListActivity {

	private ProgressDialog _progressDialog = null;
	private ArrayList<AlbumData> _albums = null;
	//private Runnable _generateAlbumList;
	private AlbumAdapter _adapter;

	/** Called when the activity is first created. */
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.dashboard);

		_albums = new ArrayList<AlbumData>();
		_adapter = new AlbumAdapter(this, R.layout.album_item, _albums);
		setListAdapter(_adapter);
		
        Thread thread = new Thread(null, new Runnable() {
			@Override
			public void run() {
				getAlbums();	
			}
		}, "RetrieveAlbums");
        thread.start();
        _progressDialog = ProgressDialog.show(this, "Please wait...", "Retrieving data ...", true);
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
	
	private void getAlbums() {
		
		try {
			Client client = User.createClient();
			_albums = new ArrayList<AlbumData>();
			UserData user = client.getUser(User.getUserName());
			for (int albumId : user.albums) {
				AlbumData album = client.getAlbum(albumId);
				_albums.add(album);
			}
			Thread.sleep(5000);
		} catch (APIException e) {
			//
		} catch (Exception e) {
			//
		}
		
		runOnUiThread(_updateAlbumList);
	}
	
	private Runnable _updateAlbumList = new Runnable() {
		@Override
		public void run() {
			if (_albums != null && _albums.size() > 0) {
				_adapter.notifyDataSetChanged();
				for (AlbumData album : _albums)
					_adapter.add(album);
			}
			_progressDialog.dismiss();
			_adapter.notifyDataSetChanged();
		}
	};

	private class AlbumAdapter extends ArrayAdapter<AlbumData> {
		private ArrayList<AlbumData> _items;

		public AlbumAdapter(Context context, int textViewResourceId,
				ArrayList<AlbumData> items) {
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
			AlbumData a = _items.get(position);
			if (a != null) {
				TextView tt = (TextView) v.findViewById(R.id.toptext);
				TextView bt = (TextView) v.findViewById(R.id.bottomtext);
				if (tt != null) {
					tt.setText(a.name);
				}
				if (bt != null) {
					bt.setText(a.description);
				}
			}
			return v;
		}
	}
}