using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Texture binding information.
    /// This is used for textures that needs to be accessed from shaders.
    /// </summary>
    struct TextureBindingInfo
    {
        /// <summary>
        /// Shader sampler target type.
        /// </summary>
        public Target Target { get; }

        /// <summary>
        /// Shader texture handle.
        /// This is an index into the texture constant buffer.
        /// </summary>
        public int Handle { get; }

        /// <summary>
        /// Indicates if the texture is a bindless texture.
        /// </summary>
        /// <remarks>
        /// For those textures, Handle is ignored.
        /// </remarks>
        public bool IsBindless { get; }

        /// <summary>
        /// Constant buffer slot with the bindless texture handle, for bindless texture.
        /// </summary>
        public int CbufSlot { get; }

        /// <summary>
        /// Constant buffer offset of the bindless texture handle, for bindless texture.
        /// </summary>
        public int CbufOffset { get; }

        /// <summary>
        /// Constructs the texture binding information structure.
        /// </summary>
        /// <param name="target">The shader sampler target type</param>
        /// <param name="handle">The shader texture handle (read index into the texture constant buffer)</param>
        public TextureBindingInfo(Target target, int handle)
        {
            Target = target;
            Handle = handle;

            IsBindless = false;

            CbufSlot   = 0;
            CbufOffset = 0;
        }

        /// <summary>
        /// Constructs the bindless texture binding information structure.
        /// </summary>
        /// <param name="target">The shader sampler target type</param>
        /// <param name="cbufSlot">Constant buffer slot where the bindless texture handle is located</param>
        /// <param name="cbufOffset">Constant buffer offset of the bindless texture handle</param>
        public TextureBindingInfo(Target target, int cbufSlot, int cbufOffset)
        {
            Target = target;
            Handle = 0;

            IsBindless = true;

            CbufSlot   = cbufSlot;
            CbufOffset = cbufOffset;
        }
    }
}