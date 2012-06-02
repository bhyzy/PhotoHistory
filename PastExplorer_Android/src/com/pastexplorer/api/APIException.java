package com.pastexplorer.api;

public class APIException extends Exception {
	private static final long serialVersionUID = 6936843596039415748L;

	public APIException() {}

	public APIException(String msg) {
		super(msg);
	}
	
	public APIException(String msg, Throwable throwable) {
		super(msg, throwable);
	}
	
	public APIException(Throwable throwable) {
		super(throwable);
	}
}
