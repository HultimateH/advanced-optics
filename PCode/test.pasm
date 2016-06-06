; test file
add 100f 000f 002D ; 15 + 30
add 1010 0000 FFff lotsa garbage aaaaaaaa; commenttttt

MORE GARBAGEaaaaaaaaaaaa aaaaa



badr 0001
copy 0101 100f 0000
#string 0201 0000 "test string!"
#string 0202 0000 "more test string!\nThis one _should_ be more interesting;\nIt is longer after all."
halt