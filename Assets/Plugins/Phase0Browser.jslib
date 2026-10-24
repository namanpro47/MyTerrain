mergeInto(LibraryManager.library, {
  TerraCameraStatus: function(yaw, pitch, distance, x, z) {
    var status = document.getElementById('camera-status');
    if (status) status.textContent = 'View ' + yaw.toFixed(1) + '° / ' + pitch.toFixed(1) + '° · Distance ' + distance.toFixed(1) + ' · Pan ' + x.toFixed(1) + ', ' + z.toFixed(1);
  }
});
