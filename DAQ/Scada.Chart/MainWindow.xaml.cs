﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Scada.Chart
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        CurveView view2;
        CurveDataContext c2;

        private double i = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
        }

        private void ButtonReset(object sender, RoutedEventArgs e)
        {
            string cd = "0 0 0 0 0 0 0 0 0 0 0 1 42 209 365 458 527 551 547 616 566 663 711 693 695 753 776 769 781 840 855 864 870 973 968 1065 1064 1086 1056 1140 1161 1196 1236 1288 1265 1377 1346 1369 1339 1348 1335 1464 1434 1389 1491 1446 1337 1418 1314 1320 1339 1330 1334 1324 1350 1248 1321 1322 1320 1282 1308 1299 1241 1159 1162 1175 1219 1171 1227 1180 1145 1181 1173 1178 1169 1133 1109 1179 1146 1165 1104 1139 1143 1115 1120 1077 1058 1094 1095 1054 1044 1027 990 1053 1004 964 1034 978 1003 915 955 934 954 975 947 935 903 899 878 846 917 845 856 922 863 809 859 820 829 835 819 853 811 822 759 808 730 865 780 800 801 765 822 747 768 852 759 761 791 768 853 783 797 834 829 794 820 803 822 816 777 746 759 733 744 750 751 800 702 736 714 758 735 704 737 677 688 665 726 656 686 701 704 711 660 711 652 676 684 696 682 686 625 689 623 697 686 612 655 684 700 706 673 646 645 647 634 657 650 625 636 649 628 669 637 640 606 622 625 643 609 620 600 631 585 621 614 637 624 646 591 583 619 592 590 601 607 594 558 583 623 623 570 585 605 548 588 555 572 583 577 571 617 542 545 544 571 614 564 574 522 538 562 561 543 576 582 565 527 557 573 522 497 553 494 567 511 508 518 496 521 531 543 505 508 503 506 588 513 540 524 528 516 549 522 519 475 511 476 509 503 452 472 487 517 456 521 480 466 505 478 496 464 481 464 482 473 449 459 447 471 453 493 466 476 480 440 456 454 438 472 437 458 429 422 469 444 456 449 439 469 436 470 435 442 447 433 413 466 445 424 439 432 440 486 412 455 415 453 429 426 427 417 394 399 415 437 382 398 446 423 408 403 426 406 424 419 415 419 422 413 433 416 443 427 428 428 442 402 442 420 430 423 404 461 408 476 408 473 394 426 424 426 458 433 430 437 425 425 419 447 414 423 455 433 440 453 504 510 460 494 509 493 529 503 513 558 561 580 535 576 591 624 678 666 673 727 720 701 738 744 724 764 775 729 730 789 764 735 711 735 708 646 689 639 569 587 559 562 522 476 475 498 494 436 444 418 433 421 421 392 391 362 384 367 388 401 401 401 410 409 379 400 362 417 371 392 371 407 356 393 393 380 387 392 400 400 382 384 385 370 418 379 368 382 384 405 373 385 393 406 392 394 409 411 383 396 372 390 382 385 404 423 387 418 406 398 366 371 358 396 392 409 376 404 398 411 407 398 407 422 398 386 408 412 410 436 399 403 409 418 411 391 413 404 412 419 439 417 396 386 384 404 454 418 395 410 434 434 409 414 425 429 415 402 411 440 406 431 468 416 443 399 407 409 462 460 427 454 454 431 442 464 406 475 425 389 424 445 424 437 470 432 395 390 427 423 415 382 398 419 382 431 387 377 412 364 399 382 400 398 393 369 395 341 358 371 346 393 369 325 347 344 345 306 328 323 303 335 330 297 317 333 340 312 298 312 298 323 318 287 280 295 249 273 262 300 312 273 294 295 249 274 300 305 270 290 291 257 284 275 255 277 265 275 255 292 253 282 276 252 280 271 272 264 227 275 259 292 260 267 274 270 257 270 284 256 263 296 292 272 261 279 264 260 244 263 256 277 257 274 279 266 269 273 244 284 265 256 291 261 263 280 278 301 316 321 327 303 315 335 358 382 380 426 490 475 506 500 546 525 587 661 676 718 731 837 791 877 881 950 1023 977 1142 1127 1171 1274 1225 1302 1353 1345 1285 1452 1381 1442 1348 1395 1392 1385 1417 1378 1351 1387 1286 1264 1238 1182 1096 1104 1004 1038 998 882 824 752 740 653 615 583 593 472 485 430 398 344 321 277 251 277 191 178 191 172 188 131 167 152 100 119 115 111 128 139 134 143 145 138 142 162 132 158 162 179 193 199 204 237 236 261 267 281 302 325 366 359 414 420 456 473 533 563 538 630 663 701 739 760 843 889 859 932 940 989 1048 993 1107 1066 1114 1107 1157 1132 1140 1172 1121 1197 1173 1129 1140 1136 1102 1088 1010 1036 963 915 940 829 823 776 754 719 653 626 553 517 465 433 404 381 384 321 271 290 252 216 199 155 158 140 116 144 99 91 93 81 89 79 76 61 49 64 51 36 56 51 46 48 38 47 50 49 41 37 43 35 45 42 48 49 53 46 44 36 48 52 46 43 44 43 47 48 45 38 45 54 50 43 40 37 42 36 33 26 30 28 41 36 41 31 46 39 22 31 29 45 24 30 33 25 27 29 21 27 24 22 21 18 36 25 21 28 30 20 14 28 17 19 15 16 20 24 22 22 34 20 18 15 28 18 30 25 15 17 24 10 15 20 20 17 14 26 18 27 18 19 15 25 18 23 19 26 19 21 27 19 22 18 17 24 22 21 28 20 19 22 18 17 19 15 18 26 13 19 27 16 22 19 22 19 17 22 16 15 23 13 18 28 20 25 20 18 20 13 17 7 12 17 15 25 20 21 16 23 19 24 23 17 24 20 22 23 19 21 16 18 16 27 15 18 11 20 19 14 17 20 18 26 16 16 19 17 24 21 19 15 23 21 16 17 20 14 15 15 14 18 19 23 19 24 20 17 20 10 17 19 17 18 19 15 24 12 11 15 15 27 12 18 16 13 17 20 9 16 22 12 17 18 15 18 18 14 21 18 15 18 26 14 12 16 15 13 22 15 13 24 17 13 14 16 12 8 12 23 12 16 14 12 17 9 17 12 14 14 8 20 17 13 20 15 13 19 17 14 10 17 10 17 12 12 13 13 9 12 15 20 13 11 16 8 14 18 18 14 14 14 11 13 14 15 20 18 15 15 13 17 15 10 16 5 16 6 17 14 13 15 19 13 14 14 12 13 13 15 11 18 16 7 18 13 16 16 12 8 11 8 21 8 15 14 15 11 9 14 17 12 15 8 12 16 16 16 17 10 11 16 9 20 20 19 9 12 11 12 14 15 16 20 15 12 11 12 14 11 14 16 13 16 8 8 17 10 11 15 14 3 10 6 15 13 11 16 16 19 16 6 10 7 16 10 14 17 16 13 11 15 12 17 13 11 12 13 11 18 15 12 12 17 12 10 11 22 11 10 15 16 9 14 12 14 9 16 9 8 16 14 13 9 9 16 10 11 11 10 10 10 16 8 16 15 14 10 12 11 15 15 11 11 6 10 18 12 5 13 9 19 16 2 7 13 10 16 11 8 9 12 13 6 6 4 7 8 10 8 11 8 7 9 4 8 7 7 6 7 7 11 7 8 8 3 6 5 2 6 4 4 8 7 5 11 9 5 3 13 5 4 6 2 4 2 3 5 6 2 4 3 0 0 4 2 1 2 5 3 7 5 4 4 4 7 3 5 6 3 6 3 10 6 1 6 5 9 8 7 9 7 5 8 10 10 5 12 17 11 12 12 16 9 16 12 8 14 14 17 18 17 18 18 17 25 16 23 19 17 19 21 15 16 21 21 16 26 25 23 20 25 20 27 23 29 24 34 23 29 15 20 26 22 16 20 23 17 13 25 7 12 19 18 12 17 10 10 9 4 13 9 13 10 9 10 8 5 2 7 6 9 5 8 6 6 5 3 5 0 1 4 2 1 3 3 1 2 1 1 3 0 2 4 0 1 1 3 0 0 2 1 1 0 0 1 2 0 0 1 0 0 0 1 0 0 0 0 1 0 1 2 0 0 0 0 0 0 0 0 0 0 0 0 1 1 1 0 0 1 0 0 0 0 1 0 0 0 0 1 0 1 0 0 0 0 0 0 1 0 0 1 0 0 0 0 0 0 0 2 0 0 0 0 0 0 0 0 1 0 0 1 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 0 0 1 1 0 1 0 2 0 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 1 0 0 0 1 0 0 0 0 0 0 0 0 1 1 0 0 0 0 0 1 0 0 0 0 1 0 0 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 1 0 0 0 1 0 0 0 0 0 1 0 0 0 1 0 1 1 0 0 0 1 1 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 1 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 2 0 0 0 0 0 0 0 0 1 1 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 1 0 0 0 0 1 0 0 0 0 0 0 1 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0";
            string[] cs = cd.Split(' ');
            List<int> l = new List<int>();
            foreach (var i in cs)
            {
                if (!string.IsNullOrEmpty(i))
                {
                    l.Add(int.Parse(i));

                }
            }

            int[] cds = l.ToArray<int>();

            //-4.10719E+00 1.43937E+00 1.21920E-04
            double c = -4.10719E+00;
            double b = 1.43937E+00;
            double a = 1.21920E-04;



            this.EnergyChartView.SetDataPoints(cds, a, b, c);

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.ChartView.SetValueRange(-100, 200);

            DateTime t = DateTime.Parse("2014-10-02 13:34:00");
            List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
            for (long i = 0; i < 3600 * 0.8; i += 30)
            {
                if (i > 3600 * 4.2 && i < 3600 * 6.3)
                {
                    var item1 = new Dictionary<string, object>(3);
                    item1.Add("time", t.AddSeconds(i).ToString());
                    item1.Add("doserate", (double) 100);
                    //item1.Add("doserate", null);
                    data.Add(item1);
                    continue;
                }
                var item = new Dictionary<string, object>(3);
                item.Add("time", t.AddSeconds(i).ToString());
                //item.Add("doserate", (double) (3600 * 24 - i) / 3600.0);
                item.Add("doserate", (double)100);
                data.Add(item);
            }
            this.ChartView.Interval = 30;
            this.ChartView.SetDataSource(data, "doserate", t, t.AddDays(1));
        }
    }
}

