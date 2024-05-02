mergeInto(LibraryManager.library, {
  SaveBufferAsBlob: function (bufferPointer, bufferSize) {
    const bytes = new Uint8Array(bufferSize);
    for (let i = 0; i < bufferSize; i += 1) {
      bytes[i] = HEAPU8[bufferPointer + i];
    }
    const blob = new Blob([bytes]);
    const blobUrl = window.URL.createObjectURL(blob);

    const returnBufferSize = lengthBytesUTF8(blobUrl) + 1;
    const returnBuffer = _malloc(returnBufferSize);
    stringToUTF8(blobUrl, returnBuffer, returnBufferSize);
    return returnBuffer;
  }
});
