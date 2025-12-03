window.moviePlayer_setPosition = function (pos) {
    const player = document.getElementById("moviePlayer");
    if (player && pos > 0) {
        player.currentTime = pos;
    }
};

let saveInterval;
window.moviePlayer_startTracking = function (movieId) {
    const player = document.getElementById("moviePlayer");
    if (!player) return;

    clearInterval(saveInterval);

    saveInterval = setInterval(() => {
        const current = player.currentTime;
        fetch(`/api/watch/update`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ movieId: movieId, currentTime: current })
        });
    }, 5000);
};
