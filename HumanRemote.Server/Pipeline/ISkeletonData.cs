using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace HumanRemote.Server.Pipeline
{
    /// <summary>
    /// Classes implementing this interface can represent skeleton data
    /// </summary>
    interface ISkeletonData
    {
        List<ISkeletonBone> Bones { get; }
    }

    interface ISkeletonBone
    {
        string Name { get; }
        Point3D StartPosition { get; }
        Point3D EndPosition { get; }
    }
}