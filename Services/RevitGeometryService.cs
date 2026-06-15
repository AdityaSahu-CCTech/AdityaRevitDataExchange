using System.Collections.Generic;
using Autodesk.DataExchange.DataModels;
using Autodesk.GeometryUtilities.MeshAPI;
using Autodesk.Revit.DB;
using DXMesh = Autodesk.GeometryUtilities.MeshAPI.Mesh;
using DXFace = Autodesk.GeometryUtilities.MeshAPI.Face;
using DXVertex = Autodesk.GeometryUtilities.MeshAPI.Vertex;
using DXColor = Autodesk.GeometryUtilities.MeshAPI.Color;

namespace AdityaRevitDataExchange.Services
{
    public class RevitGeometryService
    {
        public ElementGeometry CreateGeometry(
            Autodesk.Revit.DB.Element revitElement)
        {
            var vertices = new List<DXVertex>();
            var faces = new List<DXFace>();

            int vertexOffset = 0;

            var options = new Options
            {
                DetailLevel = ViewDetailLevel.Fine
            };

            GeometryElement geometryElement =
                revitElement.get_Geometry(options);

            if (geometryElement == null)
                return null;

            foreach (GeometryObject geometryObject in geometryElement)
            {
                ProcessGeometryObject(
                    geometryObject,
                    vertices,
                    faces,
                    ref vertexOffset,
                    revitElement);
            }

            if (vertices.Count == 0)
                return null;

            var mesh = new DXMesh
            {
                MeshColor = GetCategoryColor(revitElement),
                Vertices = vertices,
                Faces = faces
            };

            return ElementDataModel.CreateMeshGeometry(
                mesh,
                revitElement.Name);
        }

        private void ProcessGeometryObject(
            GeometryObject geometryObject,
            List<DXVertex> vertices,
            List<DXFace> faces,
            ref int vertexOffset,
            Autodesk.Revit.DB.Element revitElement)
        {
            if (geometryObject is Solid solid)
            {
                ProcessSolid(
                    solid,
                    vertices,
                    faces,
                    ref vertexOffset,
                    revitElement);
            }
            else if (geometryObject is GeometryInstance instance)
            {
                GeometryElement symbolGeometry =
                    instance.GetInstanceGeometry();

                foreach (GeometryObject child in symbolGeometry)
                {
                    ProcessGeometryObject(
                        child,
                        vertices,
                        faces,
                        ref vertexOffset,
                        revitElement);
                }
            }
        }

        private void ProcessSolid(
            Solid solid,
            List<DXVertex> vertices,
            List<DXFace> faces,
            ref int vertexOffset,
            Autodesk.Revit.DB.Element revitElement)
        {
            if (solid == null)
                return;

            if (solid.Volume <= 0)
                return;

            foreach (Autodesk.Revit.DB.Face revitFace in solid.Faces)
            {
                Autodesk.Revit.DB.Mesh mesh =
                    revitFace.Triangulate();

                for (int i = 0; i < mesh.NumTriangles; i++)
                {
                    MeshTriangle triangle =
                        mesh.get_Triangle(i);

                    XYZ p1 = triangle.get_Vertex(0);
                    XYZ p2 = triangle.get_Vertex(1);
                    XYZ p3 = triangle.get_Vertex(2);

                    vertices.Add(
                        new DXVertex(
                            p1.X,
                            p1.Y,
                            p1.Z));

                    vertices.Add(
                        new DXVertex(
                            p2.X,
                            p2.Y,
                            p2.Z));

                    vertices.Add(
                        new DXVertex(
                            p3.X,
                            p3.Y,
                            p3.Z));

                    faces.Add(
                        new DXFace
                        {
                            Corners = new List<int>
                            {
                                vertexOffset,
                                vertexOffset + 1,
                                vertexOffset + 2
                            },

                            FaceColor =
                                GetCategoryColor(
                                    revitElement)
                        });

                    vertexOffset += 3;
                }
            }
        }

        private DXColor GetCategoryColor(
            Autodesk.Revit.DB.Element element)
        {
            string category =
                element.Category?.Name ?? "";

            switch (category)
            {
                case "Walls":
                    return new DXColor(220, 220, 220, 255);

                case "Doors":
                    return new DXColor(139, 69, 19, 255);

                case "Windows":
                    return new DXColor(0, 150, 255, 255);

                case "Floors":
                    return new DXColor(170, 170, 170, 255);

                case "Structural Columns":
                    return new DXColor(255, 180, 0, 255);

                default:
                    return new DXColor(200, 200, 200, 255);
            }
        }
    }
}