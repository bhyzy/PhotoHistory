package com.pastexplorer.api;

import java.util.List;

import android.os.Parcel;
import android.os.Parcelable;

public class AlbumData/* implements Parcelable*/ {
	public int id;
	public String name;
	public String description;
	public String category;
	public boolean isPublic;
	public int rating;
	public int views;
	public List<Integer> photos;
	
	/*
	@Override
	public int describeContents() {
		return 0;
	}
	@Override
	public void writeToParcel(Parcel parcel, int flags) {
		parcel.writeInt(id);
		parcel.writeString(name);
		parcel.writeString(description);
		parcel.writeString(category);
		parcel.writeBooleanArray(new boolean[] { isPublic });
		parcel.writeInt(rating);
		parcel.writeInt(views);
		//Integer[] photosArray = photos.toArray(null);
		//parcel.writeIntArray(photosArray.);
		//parcel.write
		
	}
	*/
}
	