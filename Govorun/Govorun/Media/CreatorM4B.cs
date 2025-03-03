using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATL;
using NAudio.Wave;

namespace Govorun.Media
{
    #region Задачи (TODO).

    #endregion

    /// <summary>
    /// Статический класс создания файла m4b из файлов mp3.
    /// </summary>
    public static class CreatorM4B
    {
        private class FileItem(string filename)
        {
            public string Name = filename;

            public Track Track = new(filename);
        }

        /// <summary>
        /// Создаёт файл m4b из всех файлов mp3 в указанной папке.
        /// </summary>
        /// <param name="folderName">Папка с файлами mp3.</param>
        /// <param name="fileName">Имя файла m4b с полным путём.</param>
        /// <param name="allImages">Добавлять ли изображения из всех файлов mp3.</param>
        /// <param name="worker">BackgroundWorker в котором выполняется процесс.</param>
        /// <returns>FileInfo для полученного файла m4b или null если была ошибка.</returns>
        /// <remarks>
        /// Если параметр allImages равно true, то будут добавлены изображения из всех файлов mp3. Иначе только из первого.
        /// </remarks>
        public static FileInfo? Create(string folderName, string fileName, bool allImages, BackgroundWorker? worker)
        {
            var folder = new DirectoryInfo(folderName);
            var fileItems = folder.GetFiles("*.mp3").Select(x => new FileItem(x.FullName)).ToArray();
            if (fileItems == null || !fileItems.Any())
            {
                return null;
            }

            var mergedMP3 = Path.GetTempFileName(); // Имя объединённого временного файла mp3.
            var mergedM4A = mergedMP3 + ".m4a";     // Имя объединённого временного файла m4a.

            // Создаём объединённый временный файл mp3, состоящий из всех файлов mp3 в папке.
            try
            {
                using (var stream = new FileStream(mergedMP3, FileMode.OpenOrCreate))
                {
                    foreach (var file in fileItems)
                    {
                        var reader = new Mp3FileReader(file.Name);
                        if ((stream.Position == 0) && (reader.Id3v2Tag != null))
                        {
                            stream.Write(reader.Id3v2Tag.RawData, 0, reader.Id3v2Tag.RawData.Length);
                        }
                        Mp3Frame frame;
                        while ((frame = reader.ReadNextFrame()) != null)
                        {
                            stream.Write(frame.RawData, 0, frame.RawData.Length);
                        }
                    }
                }

                // Перекодируем временный файл mp3 в m4a.
                // Напрямую кодировать в m4b нельзя.
                // Перекодировщик требует расширение m4a.
                using (var stream = new MediaFoundationReader(mergedMP3))
                {
                    MediaFoundationEncoder.EncodeToAac(stream, mergedM4A);
                }

                // Перемещаем файл временный файл m4a в целевую папку с целевым именем файла m4b.
                File.Move(mergedM4A, fileName, true);

                // Формируем и записываем тег в итоговый файл m4b.
                var firstTrack = fileItems.First().Track;

                Settings.MP4_createNeroChapters = true;
                Settings.MP4_createQuicktimeChapters = true;

                var aacTrack = new Track(fileName);

                aacTrack.Album = firstTrack.Album;
                aacTrack.AlbumArtist = firstTrack.AlbumArtist;
                aacTrack.Artist = firstTrack.Artist;
                aacTrack.Comment = firstTrack.Comment;
                aacTrack.Date = firstTrack.Date;
                aacTrack.Description = firstTrack.Description;
                aacTrack.Genre = firstTrack.Genre;
                aacTrack.Title = firstTrack.Album;
                aacTrack.Language = firstTrack.Language;
                aacTrack.Year = firstTrack.Year;

                // Изображения обложки.
                if (allImages)
                {
                    // Добавляем изображения из всех файлов MP3.
                    foreach (var fileItem in fileItems)
                    {
                        foreach (var picture in fileItem.Track.EmbeddedPictures)
                        {
                            aacTrack.EmbeddedPictures.Add(picture);
                        }
                    }
                }
                else
                {
                    // Добавляем изображения только из первого файла MP3.
                    foreach (var picture in firstTrack.EmbeddedPictures)
                    {
                        aacTrack.EmbeddedPictures.Add(picture);
                    }
                }

                // Содержание (разделы).
                var timestamp = new TimeSpan();
                foreach (var fileItem in fileItems)
                {
                    var chapter = new ChapterInfo
                    {
                        Title = fileItem.Track.Title,
                        StartTime = (uint)timestamp.TotalMilliseconds,
                        StartOffset = (uint)timestamp.TotalMilliseconds
                    };
                    timestamp += TimeSpan.FromMilliseconds(fileItem.Track.DurationMs);
                    chapter.EndTime = (uint)timestamp.TotalMilliseconds;
                    chapter.EndOffset = (uint)timestamp.TotalMilliseconds;
                    aacTrack.Chapters.Add(chapter);
                }

                aacTrack.Save();
                return new FileInfo(fileName);
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                File.Delete(mergedMP3);
                File.Delete(mergedM4A);
            }
        }
    }
}
