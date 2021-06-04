# Describtion
## Описание
Простая игра Шахматы. Реализована как консольная версия, так и оконная версия игры (WPF)
### WPF
![img](images/wcmd67vQBDo.jpg)
### Console
![alt text](images/11oHN91EaFc.jpg)
## Структура проекта
- **Chess/ChessBoard**
    - WPF проект (MVVM)
- **Chess/ChessLib**
    - Библиотека классов. Здесь реализована логика движения фигур.
- **Chess/chess**
    - Консольное приложение
- **Chess/svg**
    - Ресурсы для фигур в WPF (векторные картинки)
## Планы на будущее
- [x] Консольные шахматы
- [X] WPF
- [ ] Мультиплеер (ASP.NET Core MVC)
- [ ] Шахматный движок, просчитывающий ходы наперед и анализирующий игровую ситуацию
- [ ] Возможность играть против AI
## Как запустить код
Достаточно просто скачать репозиторий и запустить решение в Visual Studio, выбрать проект.
В **консольной версии** нужно выбрать желаемую фигуру из списка доступных (нажать ее номер на клавиатуре), затем выбрать желаемый ход из доступных.
## Модели шахматных фигур
В **WPF версии** используются модели шахматных фигур (картинки векторные), скачанные из общедоступного сайта https://www.iconfinder.com/
  
  В **Консольной версии** шахматные фигуры обозначаются буквами:
- p - Пешка (pawn)
- r - Ладья (rook)
- b - Слон (bishop)
- n - Конь (knight)
- q - Ферзь (Queen)
- k - Король (King)
  
  **Белые фигуры с большой буквы, черные - с маленькой.**
