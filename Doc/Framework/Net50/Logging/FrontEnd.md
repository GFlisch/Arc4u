# UI

## WPF
When working in WPF, you can write to both Files and Splunk.</br>
The integration for WPF works the same way as it does for the backend.

If you want to use WPF, there is one important step:</br>
you must delete *System.Diagnostic* in the *web.config*.

## Xamarin
In order to log to Xamarin, we use a custom serilog to store the data in *Realm.io* .

There is the option to export your logs to Excel so you can use them in mails.

If you want to use the sinks in Xamarin, you must add the following package:
- Arc4u.Standard.diagnostics.Serilog.Sinks.RealmDb

Within it, there is also an anomizer.</br>
This is used to remove the username from the log before sending it.

## UWP
As it stands right now, there is no sink integration yet for UWP.
