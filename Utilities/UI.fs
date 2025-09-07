namespace Utilities

module UI =
    open System

    let rec menu (title: string) (options: (string * 'a) list): 'a =
        let mutable currentIndex = 0
        let key = ref ConsoleKey.NoName

        Console.CursorVisible <- false

        while key.Value <> ConsoleKey.Enter do
            Console.Clear()
            Console.WriteLine title

            for index in 0 .. options.Length - 1 do
                if index = currentIndex then
                    Console.ForegroundColor <- ConsoleColor.Black
                    Console.BackgroundColor <- ConsoleColor.White
                    Console.WriteLine $"> {fst options[index]}"
                    Console.ResetColor()
                else
                    Console.WriteLine $"  {fst options[index]}"

            key.Value <- Console.ReadKey(true).Key

            match key.Value with
            | ConsoleKey.UpArrow when currentIndex > 0 -> 
                currentIndex <- currentIndex - 1
            | ConsoleKey.DownArrow when currentIndex < options.Length - 1 -> 
                currentIndex <- currentIndex + 1
            | _ -> 
                ()

        snd options[currentIndex]