﻿using FFmpeg.AutoGen;
using System;

namespace Ryujinx.Graphics.Nvdec.H264
{
    unsafe class FFmpegContext : IDisposable
    {
        private readonly AVCodec* _codec;
        private AVPacket* _packet;
        private AVCodecContext* _context;

        public FFmpegContext()
        {
            _codec = ffmpeg.avcodec_find_decoder(AVCodecID.AV_CODEC_ID_H264);
            _context = ffmpeg.avcodec_alloc_context3(_codec);

            ffmpeg.avcodec_open2(_context, _codec, null);

            _packet = ffmpeg.av_packet_alloc();
        }

        public int DecodeFrame(Surface output, ReadOnlySpan<byte> bitstream)
        {
            // Ensure the packet is clean before proceeding
            ffmpeg.av_packet_unref(_packet);

            fixed (byte* ptr = bitstream)
            {
                _packet->data = ptr;
                _packet->size = bitstream.Length;

                int rc = ffmpeg.avcodec_send_packet(_context, _packet);

                if (rc != 0)
                {
                    return rc;
                }
            }

            return ffmpeg.avcodec_receive_frame(_context, output.Frame);
        }

        public void Dispose()
        {
            fixed (AVPacket** ppPacket = &_packet)
            {
                ffmpeg.av_packet_free(ppPacket);
            }

            ffmpeg.avcodec_close(_context);

            fixed (AVCodecContext** ppContext = &_context)
            {
                ffmpeg.avcodec_free_context(ppContext);
            }
        }
    }
}
