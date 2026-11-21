# Blueforce RGB Studio

![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-0078D6?style=flat-square&logo=windows)
![Language](https://img.shields.io/badge/Language-C%23%20(.NET%204.x)-512BD4?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-00E5A0?style=flat-square)
![Dependencies](https://img.shields.io/badge/Dependencies-Zero-FF40A5?style=flat-square)

**Blueforce RGB Studio**, `ZXWMicroChip` (`VID: 0x5566`, `PID: 0x0008`) çipini kullanan **Blueforce** (ve aynı OEM donanıma sahip Zifriend ZA63/ZA68 vb.) mekanik RGB klavyeler için sıfırdan geliştirilmiş, **bağımlılıksız, açık kaynaklı ve modern arayüzlü** bir Windows RGB kontrol uygulamasıdır.

> **English Summary:** An open-source, zero-dependency Windows desktop RGB controller with a modern custom-painted dark UI for `ZXWMicroChip` (`VID_5566&PID_0008`) mechanical keyboards (Blueforce, Zifriend ZA63/ZA68, and compatible OEM boards). Controls all **23 verified 60 FPS hardware RGB modes** natively over USB HID with 0% background CPU usage.

---

## Özellikler / Features

- **23 Doğrulanmış 60 FPS Donanım Modu:** Tüm efektler doğrudan klavyenin kendi işlemcisinde 60 FPS akıcılıkla çalışır; uygulamayı kapatsan bile seçtiğin mod kalıcı olarak devam eder.
- **Modern Koyu Stüdyo Arayüzü:** Çerçevesiz, sürüklenebilir, kategori filtreli (`Tümü`, `Dalga & Akış`, `Tuş Tepkili`, `Ortam & Sabit`) özel çizim arayüz.
- **Tam Kontrol:** Parlaklık (`%0–%100`), Hız (`Çok Yavaş – Çok Hızlı`), Akış Yönü (`Sağa / Sola`), Gökkuşağı / Tek Renk anahtarı, 12 hızlı neon renk ve Özel RGB Renk Seçici.
- **Sıfır Bağımlılık (Zero Dependencies):** Harici DLL, Electron veya kurulum gerektirmez. Windows'un yerleşik `.NET Framework 4.x` altyapısı ile tek dosya `.exe` olarak derlenir.
- **Sistem Tepsisi & Otomatik Kayıt:** Ayarların otomatik kaydedilir, istediğin zaman sistem tepsisine küçültebilirsin.

---

## Desteklenen Donanım / Supported Hardware

| Özellik | Değer |
| :--- | :--- |
| **USB Vendor ID (VID)** | `0x5566` |
| **USB Product ID (PID)** | `0x0008` |
| **Üretici / Çip** | `ZXWMicroChip` (`ZXWCustom` — `MI_02`, 65-byte HID Report) |
| **Bilinen Uyumlu Modeller** | Blueforce 60%/65% mekanik RGB klavyeler, Zifriend ZA63/ZA68 ve aynı `5566:0008` çipini kullanan OEM klavyeler |

---

## Doğrulanmış 23 Adet 60 FPS Mod / Verified Modes

| Mod ID | Türkçe Adı | Kategori | Açıklama |
| :--- | :--- | :--- | :--- |
| `0x09` | Çapraz Gökkuşağı | Dalga & Akış | 60 FPS çapraz gökkuşağı dalgası |
| `0x0A` | Gökkuşağı Girdabı | Dalga & Akış | Merkezden dönen gökkuşağı spirali |
| `0x16` | Süzülen Dalga | Dalga & Akış | Yumuşak yatay renk dalgası |
| `0x07` | Dikey Dalga | Dalga & Akış | Yukarıdan aşağıya renk geçişi |
| `0x08` | Fıskiye / Çiçek | Dalga & Akış | Ortadan fıskiye şeklinde yayılım |
| `0x02` | Mavi-Pembe Geçiş | Dalga & Akış | Sol-sağ renk geçişi |
| `0x0B` | Gelgit Tarama | Dalga & Akış | Soldan sağa tarayıp sönen akış |
| `0x0C` | Kayan Gökkuşağı | Dalga & Akış | Gidip gelen parlak gökkuşağı şeridi |
| `0x12` | Gökkuşağı Yılanı | Dalga & Akış | Satır satır yılan gibi gezen şerit |
| `0x06` | Işıltılı Akış | Dalga & Akış | Akıcı donanım efekti |
| `0x0D` | Tuş Dalgası (Ripple) | Tuş Tepkili | Bastığın tuştan dışarı renkli dalga yayılır |
| `0x0E` | Renk Değiştiren Dalga | Tuş Tepkili | Bastığın her tuşta tüm klavye yeni renge boyanır |
| `0x0F` | Tek Tuş İz | Tuş Tepkili | Sadece bastığın tuşlar yanıp söner |
| `0x10` | Yatay Lazer | Tuş Tepkili | Bastığın satır boyunca lazer ışını ateşlenir |
| `0x11` | Artı Havai Fişek | Tuş Tepkili | Bastığın tuşun etrafında artı şeklinde patlama |
| `0x01` | Spektrum Döngü | Ortam & Sabit | Tüm renkler yumuşakça sırayla gelir |
| `0x04` | Nefes Alma | Ortam & Sabit | Renkli veya tek renk nefes alma |
| `0x05` | Gökkuşağı Parıltısı | Ortam & Sabit | Dolu arka plan üzerinde renkli ışıltılar |
| `0x13` | Yıldızlı Gece | Ortam & Sabit | Parlayıp sönen yıldız efekti |
| `0x14` | Renkli Yağmur | Ortam & Sabit | Sütunlardan akan neon yağmur damlaları |
| `0x15` | Otomatik Havai Fişek | Ortam & Sabit | Rastgele renkli havai fişek patlamaları |
| `0x03` | Sabit Renk | Ortam & Sabit | Seçtiğin tek RGB renk |
| `0x00` | Işıkları Kapat | Ortam & Sabit | Tüm LED'leri kapatır |

---

## Kurulum ve Çalıştırma / Build & Run

### 1. `.exe` Olarak Derleme (Build)
Windows'ta yerleşik bulunan `.NET Framework 4.x` (`csc.exe`) ile tek komutla derleyebilirsin:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

Bu komut klasörde bağımsız **`BlueforceRGB.exe`** dosyasını üretir.

### 2. Çalıştırma (Run)
- Doğrudan **`BlueforceRGB.exe`** dosyasını çalıştırabilirsin.
- *(Not: Bilgisayarında **Windows Akıllı Uygulama Denetimi / Smart App Control** açıksa imzasız `.exe` dosyaları engellenebilir. Bu durumda hiçbir ayar değiştirmeden **`BlueforceRGB_Baslat.bat`** dosyasına çift tıklayarak uygulamayı anında çalıştırabilirsin.)*

---

## USB HID Protokol Özeti (Geliştiriciler İçin / Protocol Notes)

`VID_5566&PID_0008` (`MI_02` — `ZXWCustom`, 65 bayt giriş/çıkış raporu):

- **Çerçeveleme:** `buf[0] = 0x00` (Report ID), `buf[1] = 0x55` (Header), `buf[2] = CMD`, `buf[4] = sum(buf[5..64]) & 0xFF` (Checksum).
- **Onay (ACK):** Klavye her geçerli komuta `resp[1] == 0xAA` ve `resp[2] == CMD` ile yanıt verir.
- **Oturum Başlat / Bitir:** `CMD = 0x01` (Begin) / `CMD = 0x02` (End).
- **Ayar Okuma / Yazma:** `CMD = 0x05` (`buf[5] = 0x20` ile mevcut ışık ayarını okur), `CMD = 0x06` (ışık modunu yazar):
  - `buf[9..10]` = `0x02, 0xAA`
  - `buf[11]` = `modeId` (`0x00`–`0x16`)
  - `buf[12]` = `brightness` (`0–100`)
  - `buf[13]` = `4 - speed` (`0–4`)
  - `buf[14]` = `direction` (`0` veya `1`)
  - `buf[15]` = `colorful` (`0` tek renk, `1` gökkuşağı)
  - `buf[17..19]` = `R, G, B`
  - `buf[24..25]` = `0x01, 0x04`

---

## Proje Yapısı / Project Structure

```text
BlueforceRGB/
├── src/
│   ├── Program.cs          # Uygulama giriş noktası (DPI farkındalığı)
│   ├── MainForm.cs         # Ana pencere, kategori filtreleri ve ayar yönetimi
│   ├── ModernControls.cs   # Özel GDI+ modern kartlar, slider, toggle ve renk paleti
│   └── HidKeyboard.cs      # Native Win32 SetupAPI/HID protokol sürücüsü (5566:0008)
├── build.ps1               # Tek tıkla BlueforceRGB.exe derleme betiği
├── BlueforceRGB.ps1        # PowerShell bellek içi başlatıcı
├── BlueforceRGB_Baslat.bat # Çift tıkla başlatıcı (Smart App Control uyumlu)
├── LICENSE                 # MIT Lisansı
└── README.md
```

---

## Lisans / License

Bu proje **MIT Lisansı** altında açık kaynaklıdır. Detaylar için [LICENSE](LICENSE) dosyasına bakabilirsin.
