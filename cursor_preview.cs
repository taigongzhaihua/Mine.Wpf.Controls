using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

[STAThread]
static void Main()
{
    Save("grab_preview.png",    DrawGrab());
    Save("grabbing_preview.png", DrawGrabbing());
    Console.WriteLine("done");
}

static void Save(string path, DrawingVisual dv)
{
    const int Scale = 12;
    var rtb = new RenderTargetBitmap(32*Scale, 32*Scale, 96, 96, PixelFormats.Pbgra32);
    // white bg
    var bg = new DrawingVisual();
    using (var dc2 = bg.RenderOpen())
        dc2.DrawRectangle(Brushes.White, null, new Rect(0,0,32*Scale,32*Scale));
    rtb.Render(bg);
    // scale content
    var scaled = new DrawingVisual();
    scaled.Transform = new ScaleTransform(Scale, Scale);
    using (var dc2 = scaled.RenderOpen())
    {
        foreach (Drawing d in ((DrawingGroup)dv.Drawing).Children)
            dc2.DrawDrawing(d);
    }
    rtb.Render(scaled);
    var enc = new PngBitmapEncoder();
    enc.Frames.Add(BitmapFrame.Create(rtb));
    using var fs = File.OpenWrite(path);
    enc.Save(fs);
}

static Pen MakePen() => new(Brushes.Black, 2.0)
{
    LineJoin = PenLineJoin.Round,
    StartLineCap = PenLineCap.Round,
    EndLineCap   = PenLineCap.Round,
};

static PathGeometry RR(double x, double y, double w, double h, double r)
{
    double r2 = Math.Min(r, Math.Min(w/2, h/2));
    var fig = new PathFigure { StartPoint = new Point(x+r2,y), IsClosed=true, IsFilled=true };
    fig.Segments.Add(new LineSegment(new Point(x+w-r2,y),true));
    fig.Segments.Add(new ArcSegment(new Point(x+w,y+r2),new Size(r2,r2),0,false,SweepDirection.Clockwise,true));
    fig.Segments.Add(new LineSegment(new Point(x+w,y+h-r2),true));
    fig.Segments.Add(new ArcSegment(new Point(x+w-r2,y+h),new Size(r2,r2),0,false,SweepDirection.Clockwise,true));
    fig.Segments.Add(new LineSegment(new Point(x+r2,y+h),true));
    fig.Segments.Add(new ArcSegment(new Point(x,y+h-r2),new Size(r2,r2),0,false,SweepDirection.Clockwise,true));
    fig.Segments.Add(new LineSegment(new Point(x,y+r2),true));
    fig.Segments.Add(new ArcSegment(new Point(x+r2,y),new Size(r2,r2),0,false,SweepDirection.Clockwise,true));
    return new PathGeometry(new[]{fig});
}

static PathGeometry Thumb(double tx, double ty, double angle)
{
    var g = RR(-2.25,-1.0,4.5,10.5,2.25);
    var tg = new TransformGroup();
    tg.Children.Add(new RotateTransform(angle));
    tg.Children.Add(new TranslateTransform(tx,ty));
    g.Transform = tg;
    return g;
}

static DrawingVisual DrawGrab()
{
    var dv = new DrawingVisual();
    var pen = MakePen();
    using var dc = dv.RenderOpen();
    dc.DrawGeometry(Brushes.White, pen, RR( 1.5,13.0, 4.5,10.5,2.25));
    dc.DrawGeometry(Brushes.White, pen, RR( 7.0, 8.5, 4.5,14.0,2.25));
    dc.DrawGeometry(Brushes.White, pen, RR(12.5, 3.5, 4.5,19.0,2.25));
    dc.DrawGeometry(Brushes.White, pen, RR(18.0, 7.0, 4.5,15.5,2.25));
    dc.DrawGeometry(Brushes.White, pen, RR( 1.5,20.5,21.0, 9.0,2.5));
    dc.DrawGeometry(Brushes.White, pen, Thumb(23.5,25.5,-45));
    return dv;
}

static DrawingVisual DrawGrabbing()
{
    var dv = new DrawingVisual();
    var pen = MakePen();
    using var dc = dv.RenderOpen();
    dc.DrawGeometry(Brushes.White, pen, RR( 1.5,16.5, 4.5, 7.0,2.25));
    dc.DrawGeometry(Brushes.White, pen, RR( 7.0,12.0, 4.5,11.5,2.25));
    dc.DrawGeometry(Brushes.White, pen, RR(12.5, 8.0, 4.5,15.5,2.25));
    dc.DrawGeometry(Brushes.White, pen, RR(18.0,11.0, 4.5,12.5,2.25));
    dc.DrawGeometry(Brushes.White, pen, RR( 1.5,20.5,21.0, 9.0,2.5));
    dc.DrawGeometry(Brushes.White, pen, Thumb(23.5,25.5,-45));
    return dv;
}
