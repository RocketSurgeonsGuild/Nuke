function forEachChunk(chunks, callback, chunkSize = 50) {
    var mappedFiles = [];
    var files = chunks.concat();
    while (files.length > 0) {
        var chunk = files.splice(0, chunkSize);
        mappedFiles = mappedFiles.concat(callback(chunk));
    }
    return mappedFiles;
}

module.exports = {
    '!(*verified|*received).cs': filenames => [`dotnet .build/bin/Debug/net6.0/.build.dll lint --lint-files ${filenames.join(' ')}`],
    '*.{Shipped.txt,Unshipped.txt}': filenames => [`dotnet .build/bin/Debug/net6.0/.build.dll move-unshipped-to-shipped --lint-files ${filenames.join(' ')}`],
    '*.{csproj,targets,props,xml}': filenames => forEachChunk(filenames, chunk => [`prettier --write '${chunk.join(`' '`)}'`]),
    '*.{js,ts,jsx,tsx,json,yml,yaml}': filenames => forEachChunk(filenames, chunk => [`prettier --write '${chunk.join(`' '`)}'`]),
};
