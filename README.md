# MediaToAscii

A C# command-line tool that converts images and videos into ASCII art. Images are printed to the console (and optionally saved to a `.txt` file), while videos play back as live ASCII animations.

## Features

- Convert images (PNG, JPG, BMP, etc.) to ASCII art
- Play videos as real-time ASCII animations in the terminal
- Configurable output width
- Optional file saving for images
- Handles transparency (alpha channel) in images

## Requirements

- [.NET 6+](https://dotnet.microsoft.com/download)
- [OpenCvSharp4](https://github.com/shimat/opencvsharp) — for video processing
- `System.Drawing.Common` — for image processing

Install NuGet dependencies:

```bash
dotnet add package OpenCvSharp4
dotnet add package OpenCvSharp4.runtime.win   # or .linux / .osx
dotnet add package System.Drawing.Common
```

## Usage

```bash
MediaToAscii <path_to_file> [width] [--save]
```

### Arguments

| Argument | Description |
|----------|-------------|
| `path_to_file` | Path to an image or video file |
| `width` | (Optional) Output width in characters. Default: `100` |
| `--save` | (Optional) Save ASCII output to a `.txt` file (images only) |

### Examples

```bash
# Convert an image with default width
MediaToAscii photo.jpg

# Convert an image at width 160
MediaToAscii photo.png 160

# Convert and save to file
MediaToAscii photo.jpg 120 --save

# Play a video as ASCII animation
MediaToAscii clip.mp4 80
```

### Supported Formats

| Type | Extensions |
|------|------------|
| Images | PNG, JPG, BMP, GIF, and anything supported by `System.Drawing` |
| Videos | `.mp4`, `.avi`, `.mov`, `.mkv` |

## Controls

During video playback, press **Q** to quit.

## How It Works

Each pixel's brightness is computed using the standard luminance formula:

```
brightness = (0.299 × R + 0.587 × G + 0.114 × B) / 255
```

That value maps to a character from the ramp ` .,;:+*?%S#@` (dark → light). For images with transparency, brightness is multiplied by the alpha value. Video frames are auto-resized to fit your terminal.

## Build

```bash
dotnet build
dotnet run -- photo.jpg 120 --save
```

Self-contained executable:

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

## License

MIT
