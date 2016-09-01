using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using SimpleTestApp.Core;
using Android.Views.InputMethods;

namespace SimpleTestApp.Android
{
    [Activity(Label = "SimpleTestApp.Android", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private OELibTest oeLibTest = new OELibTest();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            Button connectButton = FindViewById<Button>(Resource.Id.ConnectButton);
            Button getServerObjectButton = FindViewById<Button>(Resource.Id.GetServerObjectButton);
            Button sendChatButton = FindViewById<Button>(Resource.Id.SendChatButton);

            EditText serverIpEdit = FindViewById<EditText>(Resource.Id.ServerIpInput);
            EditText serverPortEdit = FindViewById<EditText>(Resource.Id.ServerPortInput);
            EditText chatTextField = FindViewById<EditText>(Resource.Id.ChatText);

            TextView receivedChatStrings = FindViewById<TextView>(Resource.Id.ReceivedChatStrings);

            // Disable buttons that can only be used when connected
            sendChatButton.Enabled = false;
            chatTextField.Enabled = false;
            getServerObjectButton.Enabled = false;

            connectButton.Click += (s, e) =>
            {
                // Get server IP and port from user
                string serverIp = serverIpEdit.Text;
                int serverPort = Convert.ToInt32(serverPortEdit.Text);
                RunOnUiThread(() => connectButton.Text = $"Connecting to server at {serverIp}:{serverPort}...");

                // Connect to OELib server
                oeLibTest.Connect(serverIp, serverPort);

                oeLibTest.ClientConnected += (_, __) =>
                {
                    // Enable/disable controls affected by connection state
                    ActivateConnectedControls();

                    //Handle GetServerObjectButton clicks by requesting a test object from OELib server
                    getServerObjectButton.Click += (___, _____) =>
                    {
                        var testObject = oeLibTest.GetTestObject();
                        RunOnUiThread(() =>
                        {
                            var textView = FindViewById<TextView>(Resource.Id.GetServerObjectResultText);
                            textView.Text = $"Got test object from server. TestObject.TestString = {testObject.TestString}";
                        });
                    };
                };


                // Handle chat send button clicks
                sendChatButton.Click += (_, __) =>
                {
                    SendChatString(chatTextField, receivedChatStrings);
                };

                //Treat "Enter" the same way as a send button click
                chatTextField.KeyPress += (_, e1) =>
                    {
                        e1.Handled = false;
                        if (e1.Event.Action == KeyEventActions.Down && e1.KeyCode == Keycode.Enter)
                        {
                            SendChatString(chatTextField, receivedChatStrings);
                            e1.Handled = true;
                        }
                    };

                // Subscribe to received chat messages
                oeLibTest.ReceivedChatString += OeLibTest_ReceivedChatString;
            };
        }

        private void SendChatString(EditText chatTextField, TextView receivedChatStrings)
        {
            // Send the chat string and add it to the chat log
            oeLibTest.SendChatMessageToServer(chatTextField.Text);
            RunOnUiThread(() => receivedChatStrings.Text += $"Client: {chatTextField.Text}\n");
            chatTextField.Text = "";
        }

        /// <summary>
        /// Activate controls that needs connections to be useable
        /// </summary>
        private void ActivateConnectedControls()
        {
            Button connectButton = FindViewById<Button>(Resource.Id.ConnectButton);
            Button getServerObjectButton = FindViewById<Button>(Resource.Id.GetServerObjectButton);
            getServerObjectButton.Enabled = false;

            EditText chatTextField = FindViewById<EditText>(Resource.Id.ChatText);
            chatTextField.Enabled = false;

            Button sendChatButton = FindViewById<Button>(Resource.Id.SendChatButton);
            sendChatButton.Enabled = false;

            RunOnUiThread(() =>
            {
                // When connected there is no need to connect again
                connectButton.Text = "Connected to OELib server!";
                connectButton.Enabled = false;

                // Activate the button for requesting a test object from the server
                getServerObjectButton.Enabled = true;
                getServerObjectButton.Text = "Get test object from server";

                chatTextField.Enabled = true;
                sendChatButton.Enabled = true;

            });
        }

        private void OeLibTest_ReceivedChatString(object sender, string e)
        {
            var receivedChatStrings = FindViewById<TextView>(Resource.Id.ReceivedChatStrings);

            // Add received chat string to chat log
            RunOnUiThread(() => receivedChatStrings.Text += $"Server: {e}\n");
        }
    }
}

