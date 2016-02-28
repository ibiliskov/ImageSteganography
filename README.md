# ImageSteganography
Simple console program for hiding messages into bitmap and discovering messages from it. Program use simple steganography technique of modifying LSB (least significant bit) of color components (R, G, B) of each pixel. Modifying just LSB viewer cannot detect color modification. Key is used to additionally encrypt message using XOR technique.

Usage
------

**Hiding**

	ImageSteganography hide "sample key" image.png message_to_hide.txt output.bmp

**Discovering**

	ImageSteganography discover "sample key" output.bmp message_to_be_discovered.txt