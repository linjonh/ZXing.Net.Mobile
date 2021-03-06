using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.OS;
using Android.Hardware;
using Android.Graphics;

using Android.Content;
using Android.Runtime;
using Android.Widget;

using ZXing;
using Android.Support.V4.App;

namespace ZXing.Mobile
{
	[Activity (Label = "Scanner", ConfigurationChanges=ConfigChanges.Orientation|ConfigChanges.KeyboardHidden|ConfigChanges.ScreenLayout)]
	public class ZxingActivity : FragmentActivity 
	{
		public static event Action<ZXing.Result> OnScanCompleted;
		public static event Action OnCanceled;

		public static event Action OnCancelRequested;
		public static event Action<bool> OnTorchRequested;
		public static event Action OnAutoFocusRequested;

		public static void RequestCancel ()
		{
			var evt = OnCancelRequested;
			if (evt != null)
				evt();
		}

		public static void RequestTorch (bool torchOn)
		{
			var evt = OnTorchRequested;
			if (evt != null)
				evt(torchOn);
		}

		public static void RequestAutoFocus ()
		{
			var evt = OnAutoFocusRequested;
			if (evt != null)
				evt();
		}

		public static View CustomOverlayView { get;set; }
		public static bool UseCustomView { get; set; }
		public static MobileBarcodeScanningOptions ScanningOptions { get;set; }
		public static string TopText { get;set; }
		public static string BottomText { get;set; }

		ZXingScannerFragment scannerFragment;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			this.RequestWindowFeature (WindowFeatures.NoTitle);

			this.Window.AddFlags (WindowManagerFlags.Fullscreen); //to show
			this.Window.AddFlags (WindowManagerFlags.KeepScreenOn); //Don't go to sleep while scanning


			SetContentView(Resource.Layout.zxingscanneractivitylayout);

			scannerFragment = new ZXingScannerFragment(result => {
				var evt = OnScanCompleted;
				if (evt != null)
					OnScanCompleted(result);

				this.Finish();

			}, ScanningOptions);
			scannerFragment.CustomOverlayView = CustomOverlayView;
			scannerFragment.UseCustomView = UseCustomView;
			scannerFragment.TopText = TopText;
			scannerFragment.BottomText = BottomText;

			SupportFragmentManager.BeginTransaction()
				.Replace(Resource.Id.contentFrame, scannerFragment, "ZXINGFRAGMENT")
				.Commit();

			OnCancelRequested += HandleCancelScan;
			OnAutoFocusRequested += HandleAutoFocus;
			OnTorchRequested += HandleTorchRequested;

		}

		void HandleTorchRequested(bool on)
		{
			this.SetTorch(on);
		}

		void HandleAutoFocus()
		{
			this.AutoFocus();
		}

		void HandleCancelScan()
		{
			this.CancelScan();
		}

		protected override void OnDestroy ()
		{
			OnCancelRequested -= HandleCancelScan;
			OnAutoFocusRequested -= HandleAutoFocus;
			OnTorchRequested -= HandleTorchRequested;

			base.OnDestroy ();
		}

		public override void OnConfigurationChanged (Android.Content.Res.Configuration newConfig)
		{
			base.OnConfigurationChanged (newConfig);

			Android.Util.Log.Debug("ZXING", "Configuration Changed");
		}

		public void SetTorch(bool on)
		{
			scannerFragment.SetTorch(on);
		}

		public void AutoFocus()
		{
			scannerFragment.AutoFocus();
		}

		public void CancelScan ()
		{
			Finish ();
			var evt = OnCanceled;
			if (evt !=null)
				evt();
		}

		public override bool OnKeyDown (Keycode keyCode, KeyEvent e)
		{
			switch (keyCode)
			{
			case Keycode.Back:
				CancelScan();
				break;
			case Keycode.Focus:
				return true;
			}

			return base.OnKeyDown (keyCode, e);
		}
	}

}