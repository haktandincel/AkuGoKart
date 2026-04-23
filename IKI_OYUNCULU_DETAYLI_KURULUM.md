# İki Oyunculu Oyun Kurulum Özeti

## ✅ Tamamlanan Değişiklikler

### 1. Yeni Script Dosyaları

#### `GamepadInput.cs`
**Konum**: `Assets/Karting/Scripts/KartSystems/Inputs/GamepadInput.cs`
- Gamepad/joystick girişini destekler
- Her joystick için ayrı instance
- **Özellikleri**:
  - `JoystickNumber`: 0 = Oyuncu 1, 1 = Oyuncu 2, vs.
  - Hızlandırma, frenle, dönüş ve turbo butonları destekler

#### `SplitScreenCameraManager.cs`
**Konum**: `Assets/Karting/Scripts/Utilities/SplitScreenCameraManager.cs`
- Split-screen kamera yönetimi
- Sol ekran: Oyuncu 1
- Sağ ekran: Oyuncu 2
- **Kullananlar**: `GameObject` > `Add Component` > `SplitScreenCameraManager`

#### `TwoPlayerSetupHelper.cs`
**Konum**: `Assets/Karting/Scripts/Utilities/TwoPlayerSetupHelper.cs`
- Editor helper aracı
- Oyuncu 1 kartını otomatik olarak Oyuncu 2 için çoğaltır
- Doğru GamepadInput ayarlarını uygular

### 2. Güncellenmiş Dosyalar

#### `GameFlowManager.cs`
**Değişiklikler**:
- `using System.Collections.Generic;` eklendi
- `twoPlayerMode` property'si eklendi (bool)
- `player2Kart` property'si eklendi (ArcadeKart)
- `SplitScreenCameraManager` setup kodu eklendi
- Oyuncu 2 kart referansı aranıyor

---

## 🎮 KURULUM ADIMLARI

### Adım 1: İkinci Kartı Hazırla

**Oyuncu 1 + Gamepad, Oyuncu 2 + Klavye** kurulumu için:

1. **TwoPlayerSetupHelper kullan (Tavsiye Edilen)**
   - Hierarchy'de boş GameObject oluştur
   - `TwoPlayerSetupHelper` bileşeni ekle
   - Inspector'da `player1Kart` alanına mevcut kartı atama
   - **Önemli**: `Use Keyboard For Player2` checkbox'ını ✅ işaretle
   - `Duplicate Kart for Player 2` butonuna sağ tık > çalıştır

2. **Manual Kurulum**
   - Mevcut kart objesini Ctrl+D ile çoğalt
   - Yeni kartı başka bir konuma taşı
   - Yeni karta `AlternateKeyboardInput` bileşeni ekle (Oyuncu 1 keyboard yerine)
   - Veya `GamepadInput` ekle (Joystick 1)

### Adım 2: GameFlowManager Ayarla

1. Hierarchy'de `GameFlowManager` objesini bulun
2. Inspector'da aşağıdaki ayarları yap:
   - **Two Player Mode**: ✅ Eğer (İkinci kartı eklediysen)
   - **Auto Find Karts**: ✅ (Açık bırak)
   - **Player Kart**: İlk kart objesini ata
   - **Player2 Kart**: İkinci kart objesini ata

### Adım 3: Split-Screen Kamera Kur

1. Hierarchy'de boş GameObject oluştur
2. Adını `SplitScreenCameraManager` yap
3. `Add Component` > `SplitScreenCameraManager` ekle
4. **Main Camera** alanına: Scene'deki `Main Camera`'yı ata
5. **Player 1 Camera** alanına: Oyuncu 1 kart'ındaki `vCam` (ArcadeKart.vCam) ata
6. **Player 2 Camera** alanına: Oyuncu 2 kart'ındaki `vCam` ata

### Adım 4: Cinemachine Ayarları

Her kart için (ArcadeKart bileşeni):
- **vCam**: Bu kartta takip edilecek `CinemachineVirtualCamera`'yı ata
- **nitroBar**: UI progress bar objesini ata (varsa)

---

## 🎮 KONTROLLER

### Seçenek 1: Gamepad + Gamepad
| İşlem | Oyuncu 1 | Oyuncu 2 |
|-------|----------|----------|
| Hızlandır | RT (Gamepad 1) | RT (Gamepad 2) |
| Frenle | LT (Gamepad 1) | LT (Gamepad 2) |
| Dönüş | Sol Analog (Gamepad 1) | Sol Analog (Gamepad 2) |
| Turbo | X/Y (Gamepad 1) | X/Y (Gamepad 2) |

### Seçenek 2: Gamepad + Klavye (WASD)
| İşlem | Oyuncu 1 (Gamepad) | Oyuncu 2 (Klavye) |
|-------|----------|----------|
| Hızlandır | RT | W |
| Frenle | LT | S |
| Sol | Sol Analog | A |
| Sağ | Sol Analog | D |
| Turbo | X/Y | Space |

### Oyuncu 1: Arrow Keys + Tuşlar (Alternatif Klavye Modu)
- **İleri**: ↑ (Up Arrow)
- **Geri**: ↓ (Down Arrow)
- **Sol**: ← (Left Arrow)
- **Sağ**: → (Right Arrow)
- **Turbo**: Ctrl

---

## ✅ TEST LISTESI

- [ ] Her iki kart sahneye eklendi
- [ ] GamepadInput bileşeni her iki karta eklendi
- [ ] GameFlowManager ayarları yapıldı
- [ ] Split-screen kamera manager oluşturuldu
- [ ] Oyun başlatıldı
- [ ] Her iki gamepad algılandığını doğruladı
- [ ] Sol ve sağ ekranda ayrı kart görünüyor
- [ ] Her oyuncu kendi kartını kontrol edebiliyor
- [ ] Split-screen görünüyor, overlap yok

---

## 🐛 SORUN GIDERMESİ

### Gamepads Algılanmadı
- Windows'ta cihazları kontrol et
- Kablolarını çıkar ve yeniden tak
- Unity Editor'u yeniden başlat
- `Edit > Project Settings > Input Manager`'da InputAxes kontrol et

### Klavye Girdisi Çalışmadı
- Oyuncu 2 kartında `AlternateKeyboardInput` bileşeni var mı?
- Script'te KeyCode değerleri doğru mu? (W, A, S, D, Space)
- Focus penceresi Unity Editor mi? (Bazı tuşlar başka uygulamalar tarafından yakalanabilir)

### Split-Screen Görünmüyor
- Main Camera rect'ini kontrol et: (0, 0, 0.5, 1)
- İkinci Camera rect'i kontrol et: (0.5, 0, 0.5, 1)
- İkinci Camera'nın depth'ini kontrol et (-1 olmalı)

### Kart Hareket Etmemiyor
- GamepadInput veya AlternateKeyboardInput bileşeni var mı?
- Gamepad bağlı mı veya klavye odaklanmış mı?
- ArcadeKart.SetCanMove(true) çağrıldı mı?

### Kart Yanlış Açıdan Dönüyor
- ArcadeKart'ın vCam'ı düzgün mü atandı?
- Cinemachine kameraları Follow/LookAt'lerini kontrol et

---

## 📝 NOTLAR

- **Mevcut Sistemle Uyumlu**: Tüm değişiklikler single-player modu bozmaz
- **Genişletilebilir**: 2'den fazla oyuncu eklemek için GamepadInput/AlternateKeyboardInput index'i değiştirebilirsin
- **Custom Kontroller**: Tuş bağlanışları `AlternateKeyboardInput.cs`'de veya Input Manager'da customize edilebilir
- **Yeni AlternateKeyboardInput**: WASD kontrolleri için Oyuncu 2 klavye modu
  - Oyuncu 1: Arrow Keys veya Gamepad
  - Oyuncu 2: WASD veya Gamepad
  - Her kontrol Script'te özelleştirilebilir

---

## 📚 İlgili Dosyalar

- Kaynak: `Assets/Karting/Scripts/KartSystems/`
- UI: `Assets/Karting/Scripts/UI/`
- Utilities: `Assets/Karting/Scripts/Utilities/`
- Manager: `Assets/Karting/Scripts/GameFlowManager.cs`

---

## ❓ SORULAR?

Herhangi bir soru varsa, kontrol et:
1. IKIOYUNCULU_KURULUM.md dosyasını oku
2. Script yorumlarını oku
3. GameFlowManager.cs'deki Start() fonksiyonuna bak
4. TwoPlayerSetupHelper context menu'sini kullan
