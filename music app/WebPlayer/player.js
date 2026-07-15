let player = null;

window.accessToken = "";
window.deviceId = "";

function log(message) {
    chrome.webview.postMessage(String(message));
}
//log("player.js loaded");
window.onSpotifyWebPlaybackSDKReady = () => {

   // log("Spotify SDK Loaded");

    if (!window.accessToken) {
       // log("Waiting for access token...");
        return;
    }

    createPlayer();
};

function createPlayer() {

  //  log("createPlayer() called");

    if (player) {
      //  log("Player already exists");
        return;
    }

    if (!window.Spotify) {
      //  log("Spotify SDK not loaded");
        return;
    }

    if (!window.accessToken) {
     //   log("No access token");
        return;
    }

    player = new Spotify.Player({
        name: "My WPF Player",

        getOAuthToken: cb => {
         //   log("Supplying token");
            cb(window.accessToken);
        }
    });

  //  log("Player object created");

    player.addListener("initialization_error", ({ message }) => {
        log("Initialization Error");
        //log(message);
    });

    player.addListener("authentication_error", ({ message }) => {
        log("Authentication Error");
       // log(message);
    });

    player.addListener("account_error", ({ message }) => {
        log("Account Error");
       // log(message);
    });

    player.addListener("playback_error", ({ message }) => {
        log("Playback Error");
       // log(message);
    });

    player.addListener("ready", ({ device_id }) => {
      //  log("READY");
       // log(device_id);
        window.deviceId = device_id;
    });

    player.addListener("not_ready", ({ device_id }) => {
       // log("NOT READY");
       // log(device_id);
    });

    player.connect().then(success => {
      //  log("Connect returned:");
      //  log(success);
    });
}

async function playTrack(uri) {

   // log("playTrack called");
   // log(uri);

   // log("DeviceId:");
   // log(window.deviceId);

    const response = await fetch(
        `https://api.spotify.com/v1/me/player/play?device_id=${window.deviceId}`,
        {
            method: "PUT",
            headers: {
                Authorization: "Bearer " + window.accessToken,
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                uris: [uri]
            })
        });

   // log("Status:");
   // log(response.status);

    const text = await response.text();

   // log("Response:");
    //log(text);
}


   

