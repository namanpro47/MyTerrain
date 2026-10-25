mergeInto(LibraryManager.library, {
  TerraNumericStorage: function(receiverPointer, requestPointer) {
    var receiver = UTF8ToString(receiverPointer);
    var input;
    try { input = JSON.parse(UTF8ToString(requestPointer)); } catch (_) { return; }
    var finished = false;
    var database = null;
    function reply(ok, error, value) {
      if (finished) return;
      finished = true;
      if (database) database.close();
      SendMessage(receiver, 'OnNumericStorageReply', JSON.stringify({
        operationId: input && input.operationId, ok: ok, error: error || '', value: value || ''
      }));
    }
    function code(error) {
      return error && error.name === 'QuotaExceededError' ? 'quota-exceeded' : 'storage-unavailable';
    }
    if (!input || typeof input.operationId !== 'string' || typeof input.key !== 'string'
        || input.key.indexOf('myterrain.numeric.v1/') !== 0
        || (input.operation !== 'read' && input.operation !== 'write')) {
      reply(false, 'invalid-request'); return;
    }
    var candidate = null;
    if (input.operation === 'write') {
      try {
        candidate = JSON.parse(input.value);
        if (candidate.schemaVersion !== 1 || !Number.isSafeInteger(candidate.sequence) || candidate.sequence < 1
            || !/^[a-f0-9]{64}$/.test(candidate.saveHash) || !candidate.town
            || !Array.isArray(candidate.narration)) throw new Error('invalid');
      } catch (_) { reply(false, 'invalid-envelope'); return; }
    }
    var open;
    try { open = indexedDB.open('MyTerrainNumericSavesV1', 1); }
    catch (error) { reply(false, code(error)); return; }
    open.onupgradeneeded = function() {
      if (!open.result.objectStoreNames.contains('runs')) open.result.createObjectStore('runs');
    };
    open.onerror = function() { reply(false, code(open.error)); };
    open.onblocked = function() { reply(false, 'storage-blocked'); };
    open.onsuccess = function() {
      database = open.result;
      if (finished) { database.close(); return; }
      database.onversionchange = function() { database.close(); };
      var transaction;
      try {
        transaction = input.operation === 'write'
          ? database.transaction('runs', 'readwrite', { durability: 'strict' })
          : database.transaction('runs', 'readonly');
      } catch (error) { reply(false, code(error)); return; }
      var failure = null;
      var value = '';
      transaction.onabort = function() { reply(false, failure || code(transaction.error)); };
      transaction.onerror = function() { failure = failure || code(transaction.error); };
      // Request success is not a commit. Only transaction completion acknowledges.
      transaction.oncomplete = function() { reply(true, '', value); };
      var store = transaction.objectStore('runs');
      var read;
      try { read = store.get(input.key); }
      catch (error) { failure = code(error); transaction.abort(); return; }
      read.onsuccess = function() {
        var old = read.result;
        if (input.operation === 'read') {
          if (old !== undefined && (!old || typeof old.json !== 'string' || typeof old.hash !== 'string')) {
            failure = 'corrupt-record'; transaction.abort(); return;
          }
          value = old === undefined ? '' : old.json;
          return;
        }
        var expected = input.expectedHash || null;
        if ((old === undefined ? null : old && old.hash) !== expected) {
          failure = 'write-conflict'; transaction.abort(); return;
        }
        // Get, compare and put share one readwrite transaction across all tabs.
        try { store.put({ hash: candidate.saveHash, json: input.value }, input.key); }
        catch (error) { failure = code(error); transaction.abort(); }
      };
    };
  }
});
