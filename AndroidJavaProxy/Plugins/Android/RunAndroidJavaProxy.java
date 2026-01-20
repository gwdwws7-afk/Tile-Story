package com.DefaultCompany.AndroidJavaProxyCrash;

public class RunAndroidJavaProxy {
	
	public static void InvokeCallbackAsWorkaround(MyCallback callback) {
		callback.Foo();
	}
	
	public static void InvokeCallback(MyCallback callback) {
		final int THREAD_COUNT = 32;
		
		Thread[] threads = new Thread[THREAD_COUNT];
		
		for (int i = 0; i < THREAD_COUNT; i++) {
			Thread t = new Thread(new Runnable() {
				@Override
				public void run() {
					callback.Foo();
				}
			});
			
			threads[i] = t;
		}
		
		for (int i = 0; i < THREAD_COUNT; i++) {
			threads[i].start();
		}
		
		for (int i = 0; i < THREAD_COUNT; i++) {
			try {
				threads[i].join();
			} catch (InterruptedException e) {
				e.printStackTrace();
			}
		}
	}
}
