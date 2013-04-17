using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;

namespace HumanRemote
{
    //interface IBackgroundSubtractor
    //{
    //    void Apply(Image<Bgr, byte> image, Image<Bgr, byte> fgMask, double learningRate = -1);
    //    void GetBackgroundImage(Image<Bgr, byte> image);
    //}

    //interface IBackgroundSubtractorMOG2 : IBackgroundSubtractor
    //{
    //    int History { get; set; }
    //    int NMixtures { get; set; }
    //    float BackgroundRatio { get; set; }
    //    double VarThreshold { get; set; }
    //    float VarThresholdGen { get; set; }
    //    float VarInit { get; set; }
    //    float VarMin { get; set; }
    //    float VarMax { get; set; }
    //    float ComplexityReductionThreshold { get; set; }
    //    bool DetectShadows { get; set; }
    //    byte ShadowValue { get; set; }
    //    float ShadowThreshold { get; set; }
    //}

    //class RealBackgroundSubtractorMOG2 : IBackgroundSubtractorMOG2
    //{
    //    const int DefaultHistory2 = 500; // Learning rate; alpha = 1/defaultHistory2
    //    const float DefaultVarThreshold2 = 4.0f * 4.0f;
    //    const int DefaultNMixtures2 = 5; // maximal number of Gaussians in mixture
    //    const float DefaultBackgroundRatio2 = 0.9f; // threshold sum of weights for background test
    //    const float DefaultVarThresholdGen2 = 3.0f * 3.0f;
    //    const float DefaultVarInit2 = 15.0f; // initial variance for new components
    //    const float DefaultVarMax2 = 5 * DefaultVarInit2;
    //    const float DefaultVarMin2 = 4.0f;

    //    // additional parameters
    //    const float DefaultCT2 = 0.05f; // complexity reduction prior constant 0 - no reduction of number of components
    //    const byte DefaultnShadowDetection2 = 127; // value to use in the segmentation mask for shadows, set 0 not to do shadow detection
    //    const float DefaultTau = 0.5f; // Tau - shadow threshold, see the paper for explanation

    //    private Size _frameSize;
    //    private int _frameType;
    //    private Matrix<Single> _bgmodel;
    //    private Matrix<Byte> _bgmodelUsedModes;

    //    private int _nframes;

    //    private string _name;

    //    public RealBackgroundSubtractorMOG2()
    //    {
    //        _frameSize = new Size(0, 0);
    //        _frameType = 0;

    //        _nframes = 0;
    //        History = DefaultHistory2;
    //        VarThreshold = DefaultVarThreshold2;
    //        DetectShadows = true;

    //        NMixtures = DefaultNMixtures2;
    //        BackgroundRatio = DefaultBackgroundRatio2;
    //        VarInit = DefaultVarInit2;
    //        VarMax = DefaultVarMax2;
    //        VarMin = DefaultVarMin2;

    //        VarThresholdGen = DefaultVarThresholdGen2;
    //        ComplexityReductionThreshold = DefaultCT2;
    //        ShadowValue = DefaultnShadowDetection2;
    //        ShadowThreshold = DefaultTau;
    //    }

    //    public RealBackgroundSubtractorMOG2(int history, float varThreshold, bool shadowDetection = true)
    //    {
    //        _frameSize = new Size(0, 0);
    //        _frameType = 0;

    //        _nframes = 0;
    //        History = history > 0 ? history : DefaultHistory2;
    //        VarThreshold = varThreshold > 0 ? varThreshold : DefaultVarThreshold2;
    //        DetectShadows = shadowDetection;

    //        NMixtures = DefaultNMixtures2;
    //        BackgroundRatio = DefaultBackgroundRatio2;
    //        VarInit = DefaultVarInit2;
    //        VarMax = DefaultVarMax2;
    //        VarMin = DefaultVarMin2;

    //        VarThresholdGen = DefaultVarThresholdGen2;
    //        ComplexityReductionThreshold = DefaultCT2;
    //        ShadowValue = DefaultnShadowDetection2;
    //        ShadowThreshold = DefaultTau;
    //        _name = "BackgroundSubtractor.MOG2";
    //    }

    //    public void Initialize(Size frameSize, int frameType)
    //    {
    //        _frameSize = frameSize;
    //        _frameType = frameType;
    //        _nframes = 0;

            
    //        int nChannels = CV_MAT_CN(frameType);
    //        Debug.Assert(nChannels <= CV_CN_MAX);

    //        _bgmodel = new Matrix<Single>(1, frameSize.Height * frameSize.Width * NMixtures * (2 + nChannels));
    //        _bgmodelUsedModes = new Matrix<byte>(frameSize);
    //        _bgmodelUsedModes.SetZero();
    //    }

    //    private const int CV_CN_MAX = 64;
    //    private const int CV_CN_SHIFT = 3;
    //    private const int CV_MAT_CN_MASK = ((CV_CN_MAX - 1) << CV_CN_SHIFT);
    //    private int CV_MAT_CN(int flags)
    //    {
    //        return ((((flags) & CV_MAT_CN_MASK) >> CV_CN_SHIFT) + 1);
    //    }


    //    public void Apply(Image<Bgr, byte> image, Image<Bgr, byte> fgMask, double learningRate = -1)
    //    {

    //    }

    //    public virtual void GetBackgroundImage(Image<Bgr, byte> image)
    //    {
    //        int nChannels = CV_MAT_CN(_frameType);
    //        Debug.Assert(nChannels == 3);

    //        Matrix<byte> meanBackground = new Matrix<byte>(_frameSize.Width, _frameSize.Height, 3);
    //        meanBackground.SetZero();

    //        int firstGaussianIdx = 0;
            
    //    }


    //    public int History { get; set; }

    //    public int NMixtures { get; set; }

    //    public float BackgroundRatio { get; set; }

    //    public double VarThreshold { get; set; }

    //    public float VarThresholdGen { get; set; }

    //    public float VarInit { get; set; }

    //    public float VarMin { get; set; }

    //    public float VarMax { get; set; }

    //    public float ComplexityReductionThreshold { get; set; }

    //    public bool DetectShadows { get; set; }

    //    public byte ShadowValue { get; set; }

    //    public float ShadowThreshold { get; set; }
    //}


}
