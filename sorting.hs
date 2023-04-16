import Data.List
merge :: Ord a => [a] -> [a] -> [a]
merge x [] = x
merge [] y = y
merge (x:xs) (y:ys) | x <= y = x:merge xs (y:ys)
                    | otherwise = y:merge (x:xs) ys

msort :: Ord a => [a] -> [a]
msort [] = []
msort [a] = [a]
msort xs = merge (msort (firstHalf xs)) (msort (secondHalf xs))
        where
            firstHalf xs = let { n = length xs} in take (div n 2) xs
            secondHalf xs = let { n = length xs} in drop (div n 2) xs

qsort :: Ord a => [a] -> [a]
qsort [] = []
qsort (x:xs) = qsort [y | y <- xs, y <= x] ++ (x: qsort [y | y <- xs, y > x])

gcdss :: Integral a => a -> a -> a
gcdss x y = gcdss' (abs x) (abs y)
       where
        gcdss' a 0 = a
        gcdss' a b = gcdss' b (a `rem` b)

ssort :: Ord t => [t] -> [t]
ssort [a] = [a]
ssort xs = let { x = minimum xs}
           in x : ssort (delete x xs)