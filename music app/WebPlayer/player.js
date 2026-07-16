let player = null;

window.accessToken = "";
window.deviceId = "";

function log(message) {
    chrome.webview.postMessage(String(message));
}
window.onSpotifyWebPlaybackSDKReady = () => {
    if (!window.accessToken) {
        return;
    }

    createPlayer();
};

function createPlayer() {
    if (player) {
        log("Player already exists");
        return;
    }

    if (!window.Spotify) {
        log("Spotify SDK not loaded");
        return;
    }

    if (!window.accessToken) {
        log("No access token");
        return;
    }

    player = new Spotify.Player({
        name: "My WPF Player",

        getOAuthToken: cb => {
            cb(window.accessToken);
        }
    });
    window.player = player;
    player.addListener("initialization_error", ({ message }) => {
        log("Initialization Error");
        log(message);
    });

    player.addListener("authentication_error", ({ message }) => {
        log("Authentication Error");
        log(message);
    });

    player.addListener("account_error", ({ message }) => {
        log("Account Error");
        log(message);
    });

    player.addListener("playback_error", ({ message }) => {
        log("Playback Error");
        log(message);
    });

    player.addListener("ready", ({ device_id }) => {
        window.deviceId = device_id;
    });

    player.addListener("not_ready", ({ device_id }) => {
        log("NOT READY");
    });

    player.connect().then(success => {
      //  log("Connect returned:");
      //  log(success);
    });
}
async function togglePlayer() {
    let state = await window.player.getCurrentState();

    if (!state) return;

    if (state.paused)
        await window.player.resume();
    else
        await window.player.pause();
}

async function playTrack(uri) {
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

    const text = await response.text();
}


   

