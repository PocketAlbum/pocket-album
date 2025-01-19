# Pocket Album

Pocket Album is an image storage solution designed for portability and efficiency. It compresses images into smaller sizes and stores them within a single SQLite database, making it ideal for mobile devices with limited storage space.

## Features

- **Image Compression**: Automatically compress images to reduce file size without significant quality loss.
- **SQLite Database Storage**: Store all images in a single SQLite database for easy portability and management.
- **Optimized for Mobile**: Designed to work efficiently on mobile devices, saving storage space and improving performance.
- **Cross-Platform Compatibility**: Written in .NET, the core software can run on Windows or Unix platforms.

## How does it work?

Pocket Album resizes each image from your collection to 1000 pixels dimension, then it is compressed to JPEG format with maximal size of 100 kB. For faster rendering, another smaller thumbnail is generated with 100 pixels dimension and maximal size of 10 kB. The additional information such as timestamp of creation and location is extracted to allow for easier browsing and searching through the album. Images are then inserted into an SQLite database which can be easily transferred to other devices.

## How to try it out?

1. Clone the repository.
2. Compile the PocketAlbum.Tool project using Visual Studio.
3. Run the produced executable passing name for your sqlite file as the first argument.
4. Provide path to your images when asked.

## Roadmap

- Develop a web interface for easier access.
- Implement advanced search and tagging capabilities.
- Introduce safe image archiving and backup solution.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

Feel free to adapt Pocket Album to suit your needs, and enjoy efficient, portable image storage! 🚀