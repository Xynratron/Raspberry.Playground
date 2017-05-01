namespace Bmf.Shared.Esb.Types
{
    /// <summary>
    /// ChunkedTransport indicates that the original message is split into several pieces.
    /// This may happen, because the default WCF Endpoint only supports messages up to 64k.
    /// If you need bigger messagesize you have to configure that for every endpoint. To
    /// avoid this configuraton overhead, the messeges are split.
    /// </summary>
    public enum ChunkedTransport
    {
        /// <summary>
        /// No Splitting of the message, this is the only envelope.
        /// </summary>
        none = 0,

        /// <summary>
        /// The message is split into several pieces. This is the first envelope.
        /// </summary>
        begin = 1,

        /// <summary>
        ///  The message is split into several pieces. This is a chunk with message headers. There can be more chunks with headers.
        /// </summary>
        chunkHeader = 2,

        /// <summary>
        /// The message is split into several pieces. This is a envelope with the data. There can be more chunks.
        /// </summary>
        chunkData = 3,

        /// <summary>
        /// The message is split into several pieces. This is the last envelope.
        /// </summary>
        end = 4
    }
}