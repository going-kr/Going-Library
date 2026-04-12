using Going.UI.FlowSystem;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Going.UI.Tools
{
    /// <summary>
    /// FlowSystem 배관 경로 계산 유틸리티 클래스입니다.
    /// 포트 간 연결선 생성, 노드 정규화, 스무딩, 위치 보간 기능을 제공합니다.
    /// </summary>
    public static class PipeTool
    {
        #region Lines
        /// <summary>
        /// 시작 포트에서 노드 목록을 거쳐 끝 포트까지의 직선 경로 점 목록을 생성합니다.
        /// </summary>
        /// <param name="start">시작 연결 포트</param>
        /// <param name="nodes">중간 배관 노드 목록</param>
        /// <param name="end">끝 연결 포트 (null 허용)</param>
        /// <returns>경로를 구성하는 점 목록</returns>
        public static List<SKPoint> Lines(ConnectPort start, List<PipeNode> nodes, ConnectPort? end)
        {
            var points = new List<SKPoint>();
            points.Add(start.Position);

            float currentX = start.Position.X;
            float currentY = start.Position.Y;

            foreach (var node in nodes)
            {
                switch (node.Direction)
                {
                    case PortDirection.L: currentX = node.Position; break;
                    case PortDirection.R: currentX = node.Position; break;
                    case PortDirection.T: currentY = node.Position; break;
                    case PortDirection.B: currentY = node.Position; break;
                }

                var newPoint = new SKPoint(currentX, currentY);

                var lastPoint = points[points.Count - 1];
                if (Math.Abs(lastPoint.X - newPoint.X) > 0.001f ||
                    Math.Abs(lastPoint.Y - newPoint.Y) > 0.001f)
                    points.Add(newPoint);
            }

            if (end != null)
            {
                var lastPoint = points[points.Count - 1];
                if (Math.Abs(lastPoint.X - end.Position.X) > 0.001f ||
                    Math.Abs(lastPoint.Y - end.Position.Y) > 0.001f)
                {
                    var pt = end.Position;
                    points.Add(pt);
                }
            }

            return points;
        }
        #endregion

        #region Normalize
        /// <summary>
        /// 배관 노드 목록을 정규화합니다. 연속된 같은 방향의 노드를 병합하고,
        /// 끝 포트 위치에 맞게 노드 좌표를 조정합니다.
        /// </summary>
        /// <param name="start">시작 연결 포트</param>
        /// <param name="nodes">정규화할 배관 노드 목록</param>
        /// <param name="end">끝 연결 포트</param>
        /// <returns>정규화된 배관 노드 목록</returns>
        public static List<PipeNode> Normalize(ConnectPort start, List<PipeNode> nodes, ConnectPort end)
        {
            if (nodes.Count == 0)
            {
                var result = new List<PipeNode>();
                float firstPos = (start.Direction == PortDirection.L || start.Direction == PortDirection.R)
                                 ? end.Position.X : end.Position.Y;
                result.Add(new PipeNode { Direction = start.Direction, Position = firstPos });
                return result;
            }

            var normalized = new List<PipeNode>();
            normalized.Add(new PipeNode { Direction = nodes[0].Direction, Position = nodes[0].Position });
            for (int i = 1; i < nodes.Count; i++)
            {
                var current = nodes[i];
                var last = normalized.Last();
                if (current.Direction == last.Direction) last.Position = current.Position;
                else normalized.Add(new PipeNode { Direction = current.Direction, Position = current.Position });
            }

            if (normalized.Count > 0)
            {
                bool isEndH = (end.Direction == PortDirection.L || end.Direction == PortDirection.R);

                var lastNode = normalized.Last();
                if (lastNode.Direction == PortDirection.L || lastNode.Direction == PortDirection.R)
                    lastNode.Position = end.Position.X;
                else
                    lastNode.Position = end.Position.Y;

                var targetNode = normalized.LastOrDefault(n =>
                    (n.Direction == PortDirection.L || n.Direction == PortDirection.R) != isEndH);

                if (targetNode != null)
                {
                    if (targetNode.Direction == PortDirection.L || targetNode.Direction == PortDirection.R)
                        targetNode.Position = end.Position.X;
                    else
                        targetNode.Position = end.Position.Y;
                }
            }

            return normalized;
        }
        #endregion

        #region Smooth
        /// <summary>
        /// 직선 경로의 꺾이는 부분에 베지어 곡선을 적용하여 부드러운 경로를 생성합니다.
        /// </summary>
        /// <param name="start">시작 연결 포트 (null 허용)</param>
        /// <param name="end">끝 연결 포트 (null 허용)</param>
        /// <param name="points">직선 경로 점 목록</param>
        /// <param name="radius">곡선 반지름 (기본값: 10)</param>
        /// <returns>스무딩 처리된 점 목록</returns>
        public static List<SKPoint> Smooth(ConnectPort? start, ConnectPort? end, List<SKPoint> points, float radius = 10f)
        {
            if (points.Count < 3) return points;

            var smoothed = new List<SKPoint>();

            if (start != null)
            {
                var pt = points[0];
                if (start.Direction == PortDirection.L) pt.Offset(1, 0);
                if (start.Direction == PortDirection.R) pt.Offset(-1, 0);
                if (start.Direction == PortDirection.T) pt.Offset(0, 1);
                if (start.Direction == PortDirection.B) pt.Offset(0, -1);
                smoothed.Add(pt);
            }

            smoothed.Add(points[0]);

            for (int i = 1; i < points.Count - 1; i++)
            {
                var prev = points[i - 1];
                var curr = points[i];
                var next = points[i + 1];

                var v1 = new SKPoint(curr.X - prev.X, curr.Y - prev.Y);
                var len1 = (float)Math.Sqrt(v1.X * v1.X + v1.Y * v1.Y);

                var v2 = new SKPoint(next.X - curr.X, next.Y - curr.Y);
                var len2 = (float)Math.Sqrt(v2.X * v2.X + v2.Y * v2.Y);

                var actualRadius = Math.Min(radius, Math.Min(len1, len2) * 0.5f);

                v1.X /= len1;
                v1.Y /= len1;
                v2.X /= len2;
                v2.Y /= len2;

                var cornerStart = new SKPoint(
                    curr.X - v1.X * actualRadius,
                    curr.Y - v1.Y * actualRadius
                );

                var cornerEnd = new SKPoint(
                    curr.X + v2.X * actualRadius,
                    curr.Y + v2.Y * actualRadius
                );

                smoothed.Add(cornerStart);

                int segments = 8;
                for (int j = 1; j <= segments; j++)
                {
                    float t = j / (float)segments;
                    float t1 = 1 - t;

                    var point = new SKPoint(
                        t1 * t1 * cornerStart.X + 2 * t1 * t * curr.X + t * t * cornerEnd.X,
                        t1 * t1 * cornerStart.Y + 2 * t1 * t * curr.Y + t * t * cornerEnd.Y
                    );
                    smoothed.Add(point);
                }
            }

            smoothed.Add(points[points.Count - 1]);

            if (end != null)
            {
                var pt = points[points.Count - 1];
                if (end.Direction == PortDirection.L) pt.Offset(1, 0);
                if (end.Direction == PortDirection.R) pt.Offset(-1, 0);
                if (end.Direction == PortDirection.T) pt.Offset(0, 1);
                if (end.Direction == PortDirection.B) pt.Offset(0, -1);
                smoothed.Add(pt);
            }

            return smoothed;
        }
        #endregion

        #region Location
        /// <summary>
        /// 경로 상에서 지정된 비율(0~1) 위치의 좌표를 반환합니다.
        /// </summary>
        /// <param name="path">경로 점 목록</param>
        /// <param name="percent">경로 상 위치 비율 (0.0 ~ 1.0)</param>
        /// <returns>해당 위치의 좌표</returns>
        public static SKPoint Location(List<SKPoint> path, double percent)
        {
            if (path == null || path.Count == 0)
                throw new ArgumentException("Path must contain at least one point");

            if (path.Count == 1)
                return path[0];

            percent = Math.Max(0, Math.Min(1, percent));

            double totalLength = 0;
            for (int i = 0; i < path.Count - 1; i++)
            {
                totalLength += Distance(path[i], path[i + 1]);
            }

            double targetLength = totalLength * percent;

            double currentLength = 0;
            for (int i = 0; i < path.Count - 1; i++)
            {
                double segmentLength = Distance(path[i], path[i + 1]);

                if (currentLength + segmentLength >= targetLength)
                {
                    double segmentPercent = (targetLength - currentLength) / segmentLength;
                    return Lerp(path[i], path[i + 1], segmentPercent);
                }

                currentLength += segmentLength;
            }

            return path[path.Count - 1];
        }

        /// <summary>
        /// 경로의 전체 길이를 계산합니다.
        /// </summary>
        /// <param name="path">경로 점 목록</param>
        /// <returns>전체 경로 길이</returns>
        public static double TotalDistance(List<SKPoint> path)
        {
            if (path == null || path.Count == 0)
                throw new ArgumentException("Path must contain at least one point");

            double totalLength = 0;
            for (int i = 0; i < path.Count - 1; i++)
            {
                totalLength += Distance(path[i], path[i + 1]);
            }

            return totalLength;
        }

        /// <summary>
        /// 두 점 사이의 거리를 계산합니다.
        /// </summary>
        private static double Distance(SKPoint p1, SKPoint p2)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// 두 점 사이의 선형 보간 좌표를 반환합니다.
        /// </summary>
        private static SKPoint Lerp(SKPoint p1, SKPoint p2, double t)
        {
            return new SKPoint(
                p1.X + (float)((p2.X - p1.X) * t),
                p1.Y + (float)((p2.Y - p1.Y) * t)
            );
        }
        #endregion
    }
}
