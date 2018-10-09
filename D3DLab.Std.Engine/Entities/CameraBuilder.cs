﻿using D3DLab.Std.Engine.Components;
using D3DLab.Std.Engine.Core;
using System;
using System.Numerics;
using Veldrid;

namespace D3DLab.Std.Engine.Entities {
    public static class CameraBuilder {

        public class CameraComponent : GraphicComponent, IRenderableComponent {
            private Vector3 _position;
            private float _moveSpeed = 10.0f;

            private float _yaw;
            private float _pitch;

            private Vector2 _previousMousePos;
            public float Width { get; set; }

            public float VWidth { get; set; }
            public float VHeight { get; set; }
            public float Scale { get; set; }

            public Vector3 Position { get => _position; set { _position = value; } }
            public Vector3 LookDirection { get; set; }
            public Vector3 UpDirection { get; set; }
            public Vector3 RotatePoint { get; set; }
            public Matrix4x4 ViewMatrix { get; private set; }
            public Matrix4x4 ProjectionMatrix { get; private set; }
            public float FarDistance { get; }
            public float FieldOfView { get; }
            public float NearDistance { get; }
            public float AspectRatio => VWidth / VHeight;
            public float Yaw { get => _yaw; set { _yaw = value; } }
            public float Pitch { get => _pitch; set { _pitch = value; } }

            public CameraComponent(float width, float height) {
                Width = 70;

                _position = Vector3.UnitZ * Width*2f;
                FarDistance = 100000f;
                FieldOfView = 1f;
                NearDistance = 1f;

               

                RotatePoint = Vector3.Zero;
                UpDirection = Vector3.UnitY;
                LookDirection = new Vector3(0,0,-1);
                VWidth = width;
               
                VHeight = height;
                Scale = 1;

                //Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
                //Vector3 lookDir = Vector3.Transform(-Vector3.UnitZ, lookRotation);
                //LookDirection = lookDir;

                UpdatePerspectiveMatrix(width, height);
                UpdateViewMatrix();
            }

            public void WindowResized(float width, float height) {
                VWidth = width;
                VHeight = height;
                UpdatePerspectiveMatrix(width, height);
            }

            private void UpdatePerspectiveMatrix(float width, float height) {
                var aspectRatio = width / height;
                //  ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView, width / height, NearDistance, FarDistance);

                float halfWidth = (float)(Width * 0.5f);
                float halfHeight =(float)((this.Width / aspectRatio) * 0.5f);

                ProjectionMatrix = Matrix4x4.CreateOrthographicOffCenter(-halfWidth * Scale, halfWidth * Scale, -halfHeight * Scale, halfHeight * Scale,
                    NearDistance, FarDistance);
            }

            private void UpdatePerspectiveMatrix() {
                UpdatePerspectiveMatrix(VWidth, VHeight);
            }

            private void UpdateViewMatrix() {                
                ViewMatrix = Matrix4x4.CreateLookAt(_position, _position + LookDirection, UpDirection);
            }


            #region render

            public void Update(RenderState state) {
                var factory = state.Factory;
                var cmd = state.Commands;
                var window = state.Window;

                UpdateViewMatrix();
                UpdatePerspectiveMatrix(window.Width, window.Height);

                state.Viewport.ProjectionMatrix = ProjectionMatrix;
                state.Viewport.ViewMatrix = ViewMatrix;

                factory.CreateIfNullBuffer(ref state.Viewport.ProjectionBuffer, new BufferDescription(64, BufferUsage.UniformBuffer));
                factory.CreateIfNullBuffer(ref state.Viewport.ViewBuffer, new BufferDescription(64, BufferUsage.UniformBuffer));

                cmd.UpdateBuffer(state.Viewport.ProjectionBuffer, 0, state.Viewport.ProjectionMatrix);
                cmd.UpdateBuffer(state.Viewport.ViewBuffer, 0, state.Viewport.ViewMatrix);
            }

            public void Render(RenderState state) {

            }


            #endregion

            public override string ToString() {
                return $"Pos:{_position}; LoockDirection:{LookDirection}; Scale:{Scale}";
            }

        }

        public class GraphicsComponent : GraphicComponent, IRenderableComponent {
            static float angle = 0;
            public void Update(RenderState state) {
                var window = state.Window;

                return;


                angle += 1;

                state.Viewport.ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
                    1.0f,
                    window.Width / window.Height,
                    0.1f,
                    1000f);
                var pos = Vector3.UnitZ * 2.5f;
                pos = Vector3.Transform(pos, Matrix4x4.CreateRotationY((float)((angle * Math.PI) / 180), Vector3.Zero));
                state.Viewport.ViewMatrix = Matrix4x4.CreateLookAt(pos, Vector3.Zero, Vector3.UnitY);
            }

            public void Render(RenderState state) {

            }
        }

    }
}