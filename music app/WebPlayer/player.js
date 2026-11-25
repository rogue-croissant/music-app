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
    log("CREATE PLAYER CALLED");
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
        window.playerConnected = true;

        log("SPOTIFY PLAYER READY");
        log("Device ID: " + device_id);

    });

    player.addListener("not_ready", ({ device_id }) => {

        window.playerConnected = false;

        log("SPOTIFY PLAYER NOT READY");
        log("Device ID: " + device_id);

    });
    player.addListener("player_state_changed", state => {
        log("State changed");
    })
    player.connect().then(success => {
        log("Connect returned: " + success);
    }).catch(error => {
        log("Connect error: " + error);
    });
}
async function forwardSkip() {
    await window.player.nextTrack();
}
async function backwardSkip() {
    await window.player.previousTrack();
}
async function togglePlayer() {
    let state = await window.player.getCurrentState();

    if (!state) return;

    if (state.paused)
        await window.player.resume();
    else
        await window.player.pause();
}
async function logoutPlayer() {

    if (player) {
        try {
            await player.pause();
        }
        catch (error) {
            log("Pause error: " + error);
        }

        log("Disconnecting Spotify session...");

        player.disconnect();

        window.playerConnected = false;
        window.deviceId = "";

        log("Spotify session disconnected.");
    }

    window.accessToken = "";
}

async function reconnectPlayer() {

    if (!player) {
        log("No existing player. Creating one...");
        createPlayer();
        return;
    }

    log("Reconnecting existing Spotify player...");

    const success = await player.connect();

    log("Reconnect result: " + success);

    if (!success) {
        log("Spotify reconnection failed.");
    }
}
async function playPlaylist(playlistUri, position) {
    await fetch(`https://api.spotify.com/v1/me/player/play?device_id=${window.deviceId}`,
        {
            method: "PUT",
            headers: {
                Authorization: "Bearer " + window.accessToken,
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                context_uri: playlistUri,
                offset: {
                    position: position
                }
            })
        });
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


   

