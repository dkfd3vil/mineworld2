using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MineWorld.BloomEffect
{
    public class BloomComponent
    {
        public enum IntermediateBuffer
        {
            PreBloom,
            BlurredHorizontally,
            BlurredBothWays,
            FinalResult,
        }

        private GraphicsDevice _graphicsDevice;
        private RenderTarget2D _renderTarget2;

        private Effect _bloomCombineEffect;
        private Effect _bloomExtractEffect;
        private Effect _gaussianBlurEffect;

        private RenderTarget2D _renderTarget1;
        private ResolveTexture2D _resolveTarget;

        private BloomSettings _settings = BloomSettings.PresetSettings[0];


        // Optionally displays one of the intermediate buffers used
        // by the bloom postprocess, so you can see exactly what is
        // being drawn into each rendertarget.

        private IntermediateBuffer _showBuffer = IntermediateBuffer.FinalResult;
        private SpriteBatch _spriteBatch;

        public BloomSettings Settings
        {
            get { return _settings; }
            set { _settings = value; }
        }

        public IntermediateBuffer ShowBuffer
        {
            get { return _showBuffer; }
            set { _showBuffer = value; }
        }

        /// <summary>
        ///   Load your graphics content.
        /// </summary>
        public void Load(GraphicsDevice device, ContentManager content)
        {
            _graphicsDevice = device;

            _spriteBatch = new SpriteBatch(_graphicsDevice);

            _bloomExtractEffect = content.Load<Effect>("BloomExtract");
            _bloomCombineEffect = content.Load<Effect>("BloomCombine");
            _gaussianBlurEffect = content.Load<Effect>("GaussianBlur");


            // Look up the resolution and format of our main backbuffer.
            PresentationParameters pp = _graphicsDevice.PresentationParameters;

            int width = pp.BackBufferWidth;
            int height = pp.BackBufferHeight;

            SurfaceFormat format = pp.BackBufferFormat;

            // Create a texture for reading back the backbuffer contents.
            _resolveTarget = new ResolveTexture2D(_graphicsDevice, width, height, 1, format);

            // Create two rendertargets for the bloom processing. These are half the
            // size of the backbuffer, in order to minimize fillrate costs. Reducing
            // the resolution in this way doesn't hurt quality, because we are going
            // to be blurring the bloom images in any case.
            width /= 4;
            height /= 4;

            _renderTarget1 = new RenderTarget2D(_graphicsDevice, width, height, 1, format);
            _renderTarget2 = new RenderTarget2D(_graphicsDevice, width, height, 1, format);
        }


        /// <summary>
        ///   Unload your graphics content.
        /// </summary>
        protected void Dispose()
        {
            _resolveTarget.Dispose();
            _renderTarget1.Dispose();
            _renderTarget2.Dispose();
        }

        /// <summary>
        ///   This is where it all happens. Grabs a scene that has already been rendered,
        ///   and uses postprocess magic to add a glowing bloom effect over the top of it.
        /// </summary>
        public void Draw(GraphicsDevice device)
        {
            // Resolve the scene into a texture, so we can
            // use it as input data for the bloom processing.
            _graphicsDevice.ResolveBackBuffer(_resolveTarget);

            // Pass 1: draw the scene into rendertarget 1, using a
            // shader that extracts only the brightest parts of the image.
            _bloomExtractEffect.Parameters["BloomThreshold"].SetValue(Settings.BloomThreshold);

            DrawFullscreenQuad(_resolveTarget, _renderTarget1,
                               _bloomExtractEffect,
                               IntermediateBuffer.PreBloom);

            // Pass 2: draw from rendertarget 1 into rendertarget 2,
            // using a shader to apply a horizontal gaussian blur filter.
            SetBlurEffectParameters(1.0f/_renderTarget1.Width, 0);

            DrawFullscreenQuad(_renderTarget1.GetTexture(), _renderTarget2,
                               _gaussianBlurEffect,
                               IntermediateBuffer.BlurredHorizontally);

            // Pass 3: draw from rendertarget 2 back into rendertarget 1,
            // using a shader to apply a vertical gaussian blur filter.
            SetBlurEffectParameters(0, 1.0f/_renderTarget1.Height);

            DrawFullscreenQuad(_renderTarget2.GetTexture(), _renderTarget1,
                               _gaussianBlurEffect,
                               IntermediateBuffer.BlurredBothWays);

            // Pass 4: draw both rendertarget 1 and the original scene
            // image back into the main backbuffer, using a shader that
            // combines them to produce the final bloomed result.
            _graphicsDevice.SetRenderTarget(0, null);

            EffectParameterCollection parameters = _bloomCombineEffect.Parameters;

            parameters["BloomIntensity"].SetValue(Settings.BloomIntensity);
            parameters["BaseIntensity"].SetValue(Settings.BaseIntensity);
            parameters["BloomSaturation"].SetValue(Settings.BloomSaturation);
            parameters["BaseSaturation"].SetValue(Settings.BaseSaturation);

            _graphicsDevice.Textures[1] = _resolveTarget;

            Viewport viewport = _graphicsDevice.Viewport;

            DrawFullscreenQuad(_renderTarget1.GetTexture(),
                               viewport.Width, viewport.Height,
                               _bloomCombineEffect,
                               IntermediateBuffer.FinalResult);
        }


        /// <summary>
        ///   Helper for drawing a texture into a rendertarget, using
        ///   a custom shader to apply postprocessing effects.
        /// </summary>
        private void DrawFullscreenQuad(Texture2D texture, RenderTarget2D renderTarget,
                                        Effect effect, IntermediateBuffer currentBuffer)
        {
            _graphicsDevice.SetRenderTarget(0, renderTarget);

            DrawFullscreenQuad(texture,
                               renderTarget.Width, renderTarget.Height,
                               effect, currentBuffer);

            _graphicsDevice.SetRenderTarget(0, null);
        }


        /// <summary>
        ///   Helper for drawing a texture into the current rendertarget,
        ///   using a custom shader to apply postprocessing effects.
        /// </summary>
        private void DrawFullscreenQuad(Texture2D texture, int width, int height,
                                        Effect effect, IntermediateBuffer currentBuffer)
        {
            _spriteBatch.Begin(SpriteBlendMode.None,
                              SpriteSortMode.Immediate,
                              SaveStateMode.SaveState);

            // Begin the custom effect, if it is currently enabled. If the user
            // has selected one of the show intermediate buffer options, we still
            // draw the quad to make sure the image will end up on the screen,
            // but might need to skip applying the custom pixel shader.
            if (_showBuffer >= currentBuffer)
            {
                effect.Begin();
                effect.CurrentTechnique.Passes[0].Begin();
            }

            // Draw the quad.
            _spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
            _spriteBatch.End();

            // End the custom effect.
            if (_showBuffer >= currentBuffer)
            {
                effect.CurrentTechnique.Passes[0].End();
                effect.End();
            }
        }


        /// <summary>
        ///   Computes sample weightings and texture coordinate offsets
        ///   for one pass of a separable gaussian blur filter.
        /// </summary>
        private void SetBlurEffectParameters(float dx, float dy)
        {
            // Look up the sample weight and offset effect parameters.

            EffectParameter weightsParameter = _gaussianBlurEffect.Parameters["SampleWeights"];
            EffectParameter offsetsParameter = _gaussianBlurEffect.Parameters["SampleOffsets"];

            // Look up how many samples our gaussian blur effect supports.
            int sampleCount = weightsParameter.Elements.Count;

            // Create temporary arrays for computing our filter settings.
            float[] sampleWeights = new float[sampleCount];
            Vector2[] sampleOffsets = new Vector2[sampleCount];

            // The first sample always has a zero offset.
            sampleWeights[0] = ComputeGaussian(0);
            sampleOffsets[0] = new Vector2(0);

            // Maintain a sum of all the weighting values.
            float totalWeights = sampleWeights[0];

            // Add pairs of additional sample taps, positioned
            // along a line in both directions from the center.
            for (int i = 0; i < sampleCount/2; i++)
            {
                // Store weights for the positive and negative taps.
                float weight = ComputeGaussian(i + 1);

                sampleWeights[i*2 + 1] = weight;
                sampleWeights[i*2 + 2] = weight;

                totalWeights += weight*2;

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                float sampleOffset = i*2 + 1.5f;

                Vector2 delta = new Vector2(dx, dy)*sampleOffset;

                // Store texture coordinate offsets for the positive and negative taps.
                sampleOffsets[i*2 + 1] = delta;
                sampleOffsets[i*2 + 2] = -delta;
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (int i = 0; i < sampleWeights.Length; i++)
            {
                sampleWeights[i] /= totalWeights;
            }

            // Tell the effect about our new filter settings.
            weightsParameter.SetValue(sampleWeights);
            offsetsParameter.SetValue(sampleOffsets);
        }


        /// <summary>
        ///   Evaluates a single point on the gaussian falloff curve.
        ///   Used for setting up the blur filter weightings.
        /// </summary>
        private float ComputeGaussian(float n)
        {
            float theta = Settings.BlurAmount;

            return (float) ((1.0/Math.Sqrt(2*Math.PI*theta))*
                            Math.Exp(-(n*n)/(2*theta*theta)));
        }
    }
}