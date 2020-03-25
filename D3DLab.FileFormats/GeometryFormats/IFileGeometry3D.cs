﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.FileFormats.GeometryFormats {
    public interface IFileGeometry3D {
        IReadOnlyCollection<Vector3> Positions { get; }
        IReadOnlyCollection<int> Indices { get; }
        IReadOnlyCollection<Vector2> TextureCoors { get; }
    }
}