window.setVideoTime = (video, seconds) => {
    if (video) video.currentTime = seconds;
};

window.initVideoWatcher = (video, dotNetHelper) => {
    if (!video || !dotNetHelper) return;

    let lastSent = 0;

    video.addEventListener('timeupdate', () => {
        const currentTime = video.currentTime;
        if (currentTime - lastSent >= 5) {
            dotNetHelper.invokeMethodAsync('SaveTime', currentTime)
                .catch(err => console.error("JS -> C# invoke error:", err));
            lastSent = currentTime;
        }
    });
};
