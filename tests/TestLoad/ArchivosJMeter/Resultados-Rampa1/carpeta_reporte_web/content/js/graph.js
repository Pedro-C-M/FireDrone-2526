/*
   Licensed to the Apache Software Foundation (ASF) under one or more
   contributor license agreements.  See the NOTICE file distributed with
   this work for additional information regarding copyright ownership.
   The ASF licenses this file to You under the Apache License, Version 2.0
   (the "License"); you may not use this file except in compliance with
   the License.  You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
$(document).ready(function() {

    $(".click-title").mouseenter( function(    e){
        e.preventDefault();
        this.style.cursor="pointer";
    });
    $(".click-title").mousedown( function(event){
        event.preventDefault();
    });

    // Ugly code while this script is shared among several pages
    try{
        refreshHitsPerSecond(true);
    } catch(e){}
    try{
        refreshResponseTimeOverTime(true);
    } catch(e){}
    try{
        refreshResponseTimePercentiles();
    } catch(e){}
});


var responseTimePercentilesInfos = {
        data: {"result": {"minY": 6.0, "minX": 0.0, "maxY": 341997.0, "series": [{"data": [[0.0, 6.0], [0.1, 6.0], [0.2, 6.0], [0.3, 6.0], [0.4, 6.0], [0.5, 6.0], [0.6, 6.0], [0.7, 6.0], [0.8, 6.0], [0.9, 6.0], [1.0, 6.0], [1.1, 6.0], [1.2, 6.0], [1.3, 6.0], [1.4, 6.0], [1.5, 6.0], [1.6, 6.0], [1.7, 6.0], [1.8, 6.0], [1.9, 6.0], [2.0, 6.0], [2.1, 6.0], [2.2, 6.0], [2.3, 7.0], [2.4, 7.0], [2.5, 7.0], [2.6, 7.0], [2.7, 7.0], [2.8, 7.0], [2.9, 7.0], [3.0, 7.0], [3.1, 7.0], [3.2, 7.0], [3.3, 7.0], [3.4, 7.0], [3.5, 7.0], [3.6, 8.0], [3.7, 8.0], [3.8, 8.0], [3.9, 8.0], [4.0, 8.0], [4.1, 8.0], [4.2, 8.0], [4.3, 8.0], [4.4, 8.0], [4.5, 8.0], [4.6, 8.0], [4.7, 8.0], [4.8, 8.0], [4.9, 8.0], [5.0, 8.0], [5.1, 8.0], [5.2, 8.0], [5.3, 8.0], [5.4, 8.0], [5.5, 8.0], [5.6, 8.0], [5.7, 8.0], [5.8, 8.0], [5.9, 8.0], [6.0, 8.0], [6.1, 8.0], [6.2, 8.0], [6.3, 8.0], [6.4, 8.0], [6.5, 8.0], [6.6, 8.0], [6.7, 8.0], [6.8, 8.0], [6.9, 8.0], [7.0, 8.0], [7.1, 8.0], [7.2, 8.0], [7.3, 8.0], [7.4, 8.0], [7.5, 8.0], [7.6, 8.0], [7.7, 8.0], [7.8, 8.0], [7.9, 8.0], [8.0, 8.0], [8.1, 8.0], [8.2, 8.0], [8.3, 8.0], [8.4, 8.0], [8.5, 8.0], [8.6, 8.0], [8.7, 8.0], [8.8, 8.0], [8.9, 8.0], [9.0, 8.0], [9.1, 8.0], [9.2, 8.0], [9.3, 8.0], [9.4, 8.0], [9.5, 8.0], [9.6, 8.0], [9.7, 8.0], [9.8, 8.0], [9.9, 8.0], [10.0, 8.0], [10.1, 8.0], [10.2, 8.0], [10.3, 9.0], [10.4, 9.0], [10.5, 9.0], [10.6, 9.0], [10.7, 9.0], [10.8, 9.0], [10.9, 9.0], [11.0, 9.0], [11.1, 9.0], [11.2, 9.0], [11.3, 9.0], [11.4, 9.0], [11.5, 9.0], [11.6, 9.0], [11.7, 9.0], [11.8, 9.0], [11.9, 9.0], [12.0, 9.0], [12.1, 9.0], [12.2, 9.0], [12.3, 9.0], [12.4, 9.0], [12.5, 9.0], [12.6, 9.0], [12.7, 9.0], [12.8, 9.0], [12.9, 9.0], [13.0, 9.0], [13.1, 9.0], [13.2, 9.0], [13.3, 9.0], [13.4, 9.0], [13.5, 9.0], [13.6, 9.0], [13.7, 9.0], [13.8, 9.0], [13.9, 9.0], [14.0, 9.0], [14.1, 9.0], [14.2, 9.0], [14.3, 10.0], [14.4, 10.0], [14.5, 10.0], [14.6, 10.0], [14.7, 10.0], [14.8, 10.0], [14.9, 10.0], [15.0, 10.0], [15.1, 10.0], [15.2, 10.0], [15.3, 10.0], [15.4, 10.0], [15.5, 10.0], [15.6, 10.0], [15.7, 10.0], [15.8, 10.0], [15.9, 10.0], [16.0, 10.0], [16.1, 10.0], [16.2, 10.0], [16.3, 10.0], [16.4, 10.0], [16.5, 10.0], [16.6, 10.0], [16.7, 10.0], [16.8, 10.0], [16.9, 10.0], [17.0, 10.0], [17.1, 10.0], [17.2, 10.0], [17.3, 10.0], [17.4, 10.0], [17.5, 10.0], [17.6, 10.0], [17.7, 10.0], [17.8, 10.0], [17.9, 10.0], [18.0, 10.0], [18.1, 10.0], [18.2, 10.0], [18.3, 11.0], [18.4, 11.0], [18.5, 11.0], [18.6, 11.0], [18.7, 11.0], [18.8, 11.0], [18.9, 11.0], [19.0, 11.0], [19.1, 11.0], [19.2, 11.0], [19.3, 11.0], [19.4, 11.0], [19.5, 11.0], [19.6, 11.0], [19.7, 11.0], [19.8, 11.0], [19.9, 11.0], [20.0, 11.0], [20.1, 11.0], [20.2, 11.0], [20.3, 11.0], [20.4, 11.0], [20.5, 11.0], [20.6, 11.0], [20.7, 11.0], [20.8, 11.0], [20.9, 12.0], [21.0, 12.0], [21.1, 12.0], [21.2, 12.0], [21.3, 12.0], [21.4, 12.0], [21.5, 12.0], [21.6, 12.0], [21.7, 12.0], [21.8, 12.0], [21.9, 12.0], [22.0, 12.0], [22.1, 12.0], [22.2, 12.0], [22.3, 12.0], [22.4, 12.0], [22.5, 12.0], [22.6, 12.0], [22.7, 12.0], [22.8, 12.0], [22.9, 12.0], [23.0, 12.0], [23.1, 12.0], [23.2, 12.0], [23.3, 12.0], [23.4, 12.0], [23.5, 12.0], [23.6, 12.0], [23.7, 12.0], [23.8, 12.0], [23.9, 12.0], [24.0, 12.0], [24.1, 12.0], [24.2, 12.0], [24.3, 12.0], [24.4, 12.0], [24.5, 12.0], [24.6, 12.0], [24.7, 12.0], [24.8, 12.0], [24.9, 12.0], [25.0, 12.0], [25.1, 12.0], [25.2, 12.0], [25.3, 12.0], [25.4, 12.0], [25.5, 12.0], [25.6, 12.0], [25.7, 12.0], [25.8, 13.0], [25.9, 13.0], [26.0, 13.0], [26.1, 13.0], [26.2, 13.0], [26.3, 13.0], [26.4, 13.0], [26.5, 13.0], [26.6, 13.0], [26.7, 13.0], [26.8, 13.0], [26.9, 13.0], [27.0, 13.0], [27.1, 13.0], [27.2, 13.0], [27.3, 13.0], [27.4, 13.0], [27.5, 13.0], [27.6, 13.0], [27.7, 13.0], [27.8, 13.0], [27.9, 13.0], [28.0, 13.0], [28.1, 13.0], [28.2, 13.0], [28.3, 13.0], [28.4, 13.0], [28.5, 13.0], [28.6, 13.0], [28.7, 13.0], [28.8, 13.0], [28.9, 13.0], [29.0, 13.0], [29.1, 13.0], [29.2, 13.0], [29.3, 13.0], [29.4, 14.0], [29.5, 14.0], [29.6, 14.0], [29.7, 14.0], [29.8, 14.0], [29.9, 14.0], [30.0, 14.0], [30.1, 14.0], [30.2, 14.0], [30.3, 14.0], [30.4, 14.0], [30.5, 14.0], [30.6, 14.0], [30.7, 14.0], [30.8, 14.0], [30.9, 14.0], [31.0, 14.0], [31.1, 14.0], [31.2, 14.0], [31.3, 14.0], [31.4, 14.0], [31.5, 14.0], [31.6, 14.0], [31.7, 14.0], [31.8, 14.0], [31.9, 14.0], [32.0, 15.0], [32.1, 15.0], [32.2, 15.0], [32.3, 15.0], [32.4, 15.0], [32.5, 15.0], [32.6, 15.0], [32.7, 15.0], [32.8, 15.0], [32.9, 15.0], [33.0, 15.0], [33.1, 15.0], [33.2, 15.0], [33.3, 15.0], [33.4, 15.0], [33.5, 15.0], [33.6, 15.0], [33.7, 15.0], [33.8, 15.0], [33.9, 15.0], [34.0, 15.0], [34.1, 15.0], [34.2, 15.0], [34.3, 16.0], [34.4, 16.0], [34.5, 16.0], [34.6, 16.0], [34.7, 16.0], [34.8, 16.0], [34.9, 16.0], [35.0, 16.0], [35.1, 16.0], [35.2, 16.0], [35.3, 16.0], [35.4, 16.0], [35.5, 16.0], [35.6, 17.0], [35.7, 17.0], [35.8, 17.0], [35.9, 17.0], [36.0, 17.0], [36.1, 17.0], [36.2, 17.0], [36.3, 17.0], [36.4, 17.0], [36.5, 18.0], [36.6, 18.0], [36.7, 18.0], [36.8, 18.0], [36.9, 19.0], [37.0, 19.0], [37.1, 19.0], [37.2, 19.0], [37.3, 19.0], [37.4, 20.0], [37.5, 20.0], [37.6, 20.0], [37.7, 20.0], [37.8, 25.0], [37.9, 25.0], [38.0, 25.0], [38.1, 25.0], [38.2, 25.0], [38.3, 27.0], [38.4, 27.0], [38.5, 27.0], [38.6, 27.0], [38.7, 31.0], [38.8, 31.0], [38.9, 31.0], [39.0, 31.0], [39.1, 31.0], [39.2, 33.0], [39.3, 33.0], [39.4, 33.0], [39.5, 33.0], [39.6, 36.0], [39.7, 36.0], [39.8, 36.0], [39.9, 36.0], [40.0, 73.0], [40.1, 73.0], [40.2, 73.0], [40.3, 73.0], [40.4, 73.0], [40.5, 130.0], [40.6, 130.0], [40.7, 130.0], [40.8, 130.0], [40.9, 157.0], [41.0, 157.0], [41.1, 157.0], [41.2, 157.0], [41.3, 157.0], [41.4, 158.0], [41.5, 158.0], [41.6, 158.0], [41.7, 158.0], [41.8, 159.0], [41.9, 159.0], [42.0, 159.0], [42.1, 159.0], [42.2, 159.0], [42.3, 159.0], [42.4, 159.0], [42.5, 159.0], [42.6, 159.0], [42.7, 160.0], [42.8, 160.0], [42.9, 160.0], [43.0, 160.0], [43.1, 160.0], [43.2, 161.0], [43.3, 161.0], [43.4, 161.0], [43.5, 161.0], [43.6, 161.0], [43.7, 161.0], [43.8, 161.0], [43.9, 161.0], [44.0, 161.0], [44.1, 161.0], [44.2, 161.0], [44.3, 161.0], [44.4, 161.0], [44.5, 162.0], [44.6, 162.0], [44.7, 162.0], [44.8, 162.0], [44.9, 168.0], [45.0, 168.0], [45.1, 168.0], [45.2, 168.0], [45.3, 168.0], [45.4, 286.0], [45.5, 286.0], [45.6, 286.0], [45.7, 286.0], [45.8, 533.0], [45.9, 533.0], [46.0, 533.0], [46.1, 533.0], [46.2, 533.0], [46.3, 574.0], [46.4, 574.0], [46.5, 574.0], [46.6, 574.0], [46.7, 1061.0], [46.8, 1061.0], [46.9, 1061.0], [47.0, 1061.0], [47.1, 1061.0], [47.2, 283290.0], [47.3, 283290.0], [47.4, 283290.0], [47.5, 283290.0], [47.6, 283788.0], [47.7, 283788.0], [47.8, 283788.0], [47.9, 283788.0], [48.0, 284286.0], [48.1, 284286.0], [48.2, 284286.0], [48.3, 284286.0], [48.4, 284286.0], [48.5, 284784.0], [48.6, 284784.0], [48.7, 284784.0], [48.8, 284784.0], [48.9, 285279.0], [49.0, 285279.0], [49.1, 285279.0], [49.2, 285279.0], [49.3, 285279.0], [49.4, 285780.0], [49.5, 285780.0], [49.6, 285780.0], [49.7, 285780.0], [49.8, 286276.0], [49.9, 286276.0], [50.0, 286276.0], [50.1, 286276.0], [50.2, 286276.0], [50.3, 286761.0], [50.4, 286761.0], [50.5, 286761.0], [50.6, 286761.0], [50.7, 287268.0], [50.8, 287268.0], [50.9, 287268.0], [51.0, 287268.0], [51.1, 287268.0], [51.2, 287766.0], [51.3, 287766.0], [51.4, 287766.0], [51.5, 287766.0], [51.6, 288261.0], [51.7, 288261.0], [51.8, 288261.0], [51.9, 288261.0], [52.0, 288761.0], [52.1, 288761.0], [52.2, 288761.0], [52.3, 288761.0], [52.4, 288761.0], [52.5, 289261.0], [52.6, 289261.0], [52.7, 289261.0], [52.8, 289261.0], [52.9, 289758.0], [53.0, 289758.0], [53.1, 289758.0], [53.2, 289758.0], [53.3, 289758.0], [53.4, 290253.0], [53.5, 290253.0], [53.6, 290253.0], [53.7, 290253.0], [53.8, 290753.0], [53.9, 290753.0], [54.0, 290753.0], [54.1, 290753.0], [54.2, 290753.0], [54.3, 291252.0], [54.4, 291252.0], [54.5, 291252.0], [54.6, 291252.0], [54.7, 291749.0], [54.8, 291749.0], [54.9, 291749.0], [55.0, 291749.0], [55.1, 291749.0], [55.2, 292245.0], [55.3, 292245.0], [55.4, 292245.0], [55.5, 292245.0], [55.6, 292742.0], [55.7, 292742.0], [55.8, 292742.0], [55.9, 292742.0], [56.0, 293242.0], [56.1, 293242.0], [56.2, 293242.0], [56.3, 293242.0], [56.4, 293242.0], [56.5, 293740.0], [56.6, 293740.0], [56.7, 293740.0], [56.8, 293740.0], [56.9, 294238.0], [57.0, 294238.0], [57.1, 294238.0], [57.2, 294238.0], [57.3, 294238.0], [57.4, 294735.0], [57.5, 294735.0], [57.6, 294735.0], [57.7, 294735.0], [57.8, 295237.0], [57.9, 295237.0], [58.0, 295237.0], [58.1, 295237.0], [58.2, 295237.0], [58.3, 295724.0], [58.4, 295724.0], [58.5, 295724.0], [58.6, 295724.0], [58.7, 296226.0], [58.8, 296226.0], [58.9, 296226.0], [59.0, 296226.0], [59.1, 296226.0], [59.2, 296725.0], [59.3, 296725.0], [59.4, 296725.0], [59.5, 296725.0], [59.6, 297226.0], [59.7, 297226.0], [59.8, 297226.0], [59.9, 297226.0], [60.0, 297724.0], [60.1, 297724.0], [60.2, 297724.0], [60.3, 297724.0], [60.4, 297724.0], [60.5, 298225.0], [60.6, 298225.0], [60.7, 298225.0], [60.8, 298225.0], [60.9, 298720.0], [61.0, 298720.0], [61.1, 298720.0], [61.2, 298720.0], [61.3, 298720.0], [61.4, 299218.0], [61.5, 299218.0], [61.6, 299218.0], [61.7, 299218.0], [61.8, 299714.0], [61.9, 299714.0], [62.0, 299714.0], [62.1, 299714.0], [62.2, 299714.0], [62.3, 300215.0], [62.4, 300215.0], [62.5, 300215.0], [62.6, 300215.0], [62.7, 300714.0], [62.8, 300714.0], [62.9, 300714.0], [63.0, 300714.0], [63.1, 300714.0], [63.2, 301207.0], [63.3, 301207.0], [63.4, 301207.0], [63.5, 301207.0], [63.6, 301711.0], [63.7, 301711.0], [63.8, 301711.0], [63.9, 301711.0], [64.0, 302207.0], [64.1, 302207.0], [64.2, 302207.0], [64.3, 302207.0], [64.4, 302207.0], [64.5, 302706.0], [64.6, 302706.0], [64.7, 302706.0], [64.8, 302706.0], [64.9, 303205.0], [65.0, 303205.0], [65.1, 303205.0], [65.2, 303205.0], [65.3, 303205.0], [65.4, 303702.0], [65.5, 303702.0], [65.6, 303702.0], [65.7, 303702.0], [65.8, 304136.0], [65.9, 304136.0], [66.0, 304136.0], [66.1, 304136.0], [66.2, 304136.0], [66.3, 304633.0], [66.4, 304633.0], [66.5, 304633.0], [66.6, 304633.0], [66.7, 305137.0], [66.8, 305137.0], [66.9, 305137.0], [67.0, 305137.0], [67.1, 305137.0], [67.2, 305629.0], [67.3, 305629.0], [67.4, 305629.0], [67.5, 305629.0], [67.6, 306128.0], [67.7, 306128.0], [67.8, 306128.0], [67.9, 306128.0], [68.0, 306627.0], [68.1, 306627.0], [68.2, 306627.0], [68.3, 306627.0], [68.4, 306627.0], [68.5, 307109.0], [68.6, 307109.0], [68.7, 307109.0], [68.8, 307109.0], [68.9, 307622.0], [69.0, 307622.0], [69.1, 307622.0], [69.2, 307622.0], [69.3, 307622.0], [69.4, 308125.0], [69.5, 308125.0], [69.6, 308125.0], [69.7, 308125.0], [69.8, 308617.0], [69.9, 308617.0], [70.0, 308617.0], [70.1, 308617.0], [70.2, 308617.0], [70.3, 309116.0], [70.4, 309116.0], [70.5, 309116.0], [70.6, 309116.0], [70.7, 309613.0], [70.8, 309613.0], [70.9, 309613.0], [71.0, 309613.0], [71.1, 309613.0], [71.2, 310112.0], [71.3, 310112.0], [71.4, 310112.0], [71.5, 310112.0], [71.6, 310611.0], [71.7, 310611.0], [71.8, 310611.0], [71.9, 310611.0], [72.0, 311108.0], [72.1, 311108.0], [72.2, 311108.0], [72.3, 311108.0], [72.4, 311108.0], [72.5, 311606.0], [72.6, 311606.0], [72.7, 311606.0], [72.8, 311606.0], [72.9, 312104.0], [73.0, 312104.0], [73.1, 312104.0], [73.2, 312104.0], [73.3, 312104.0], [73.4, 312588.0], [73.5, 312588.0], [73.6, 312588.0], [73.7, 312588.0], [73.8, 313101.0], [73.9, 313101.0], [74.0, 313101.0], [74.1, 313101.0], [74.2, 313101.0], [74.3, 313596.0], [74.4, 313596.0], [74.5, 313596.0], [74.6, 313596.0], [74.7, 314080.0], [74.8, 314080.0], [74.9, 314080.0], [75.0, 314080.0], [75.1, 314080.0], [75.2, 314582.0], [75.3, 314582.0], [75.4, 314582.0], [75.5, 314582.0], [75.6, 315079.0], [75.7, 315079.0], [75.8, 315079.0], [75.9, 315079.0], [76.0, 315583.0], [76.1, 315583.0], [76.2, 315583.0], [76.3, 315583.0], [76.4, 315583.0], [76.5, 316083.0], [76.6, 316083.0], [76.7, 316083.0], [76.8, 316083.0], [76.9, 316582.0], [77.0, 316582.0], [77.1, 316582.0], [77.2, 316582.0], [77.3, 316582.0], [77.4, 317082.0], [77.5, 317082.0], [77.6, 317082.0], [77.7, 317082.0], [77.8, 317583.0], [77.9, 317583.0], [78.0, 317583.0], [78.1, 317583.0], [78.2, 317583.0], [78.3, 318078.0], [78.4, 318078.0], [78.5, 318078.0], [78.6, 318078.0], [78.7, 318567.0], [78.8, 318567.0], [78.9, 318567.0], [79.0, 318567.0], [79.1, 318567.0], [79.2, 319059.0], [79.3, 319059.0], [79.4, 319059.0], [79.5, 319059.0], [79.6, 319565.0], [79.7, 319565.0], [79.8, 319565.0], [79.9, 319565.0], [80.0, 320063.0], [80.1, 320063.0], [80.2, 320063.0], [80.3, 320063.0], [80.4, 320063.0], [80.5, 320549.0], [80.6, 320549.0], [80.7, 320549.0], [80.8, 320549.0], [80.9, 321063.0], [81.0, 321063.0], [81.1, 321063.0], [81.2, 321063.0], [81.3, 321063.0], [81.4, 321547.0], [81.5, 321547.0], [81.6, 321547.0], [81.7, 321547.0], [81.8, 322051.0], [81.9, 322051.0], [82.0, 322051.0], [82.1, 322051.0], [82.2, 322051.0], [82.3, 322548.0], [82.4, 322548.0], [82.5, 322548.0], [82.6, 322548.0], [82.7, 323049.0], [82.8, 323049.0], [82.9, 323049.0], [83.0, 323049.0], [83.1, 323049.0], [83.2, 323550.0], [83.3, 323550.0], [83.4, 323550.0], [83.5, 323550.0], [83.6, 324049.0], [83.7, 324049.0], [83.8, 324049.0], [83.9, 324049.0], [84.0, 324534.0], [84.1, 324534.0], [84.2, 324534.0], [84.3, 324534.0], [84.4, 324534.0], [84.5, 325035.0], [84.6, 325035.0], [84.7, 325035.0], [84.8, 325035.0], [84.9, 325549.0], [85.0, 325549.0], [85.1, 325549.0], [85.2, 325549.0], [85.3, 325549.0], [85.4, 326049.0], [85.5, 326049.0], [85.6, 326049.0], [85.7, 326049.0], [85.8, 326546.0], [85.9, 326546.0], [86.0, 326546.0], [86.1, 326546.0], [86.2, 326546.0], [86.3, 327047.0], [86.4, 327047.0], [86.5, 327047.0], [86.6, 327047.0], [86.7, 327547.0], [86.8, 327547.0], [86.9, 327547.0], [87.0, 327547.0], [87.1, 327547.0], [87.2, 328051.0], [87.3, 328051.0], [87.4, 328051.0], [87.5, 328051.0], [87.6, 328533.0], [87.7, 328533.0], [87.8, 328533.0], [87.9, 328533.0], [88.0, 329033.0], [88.1, 329033.0], [88.2, 329033.0], [88.3, 329033.0], [88.4, 329033.0], [88.5, 329530.0], [88.6, 329530.0], [88.7, 329530.0], [88.8, 329530.0], [88.9, 330035.0], [89.0, 330035.0], [89.1, 330035.0], [89.2, 330035.0], [89.3, 330035.0], [89.4, 330542.0], [89.5, 330542.0], [89.6, 330542.0], [89.7, 330542.0], [89.8, 331040.0], [89.9, 331040.0], [90.0, 331040.0], [90.1, 331040.0], [90.2, 331040.0], [90.3, 331535.0], [90.4, 331535.0], [90.5, 331535.0], [90.6, 331535.0], [90.7, 332034.0], [90.8, 332034.0], [90.9, 332034.0], [91.0, 332034.0], [91.1, 332034.0], [91.2, 332534.0], [91.3, 332534.0], [91.4, 332534.0], [91.5, 332534.0], [91.6, 333034.0], [91.7, 333034.0], [91.8, 333034.0], [91.9, 333034.0], [92.0, 333531.0], [92.1, 333531.0], [92.2, 333531.0], [92.3, 333531.0], [92.4, 333531.0], [92.5, 334029.0], [92.6, 334029.0], [92.7, 334029.0], [92.8, 334029.0], [92.9, 334526.0], [93.0, 334526.0], [93.1, 334526.0], [93.2, 334526.0], [93.3, 334526.0], [93.4, 335026.0], [93.5, 335026.0], [93.6, 335026.0], [93.7, 335026.0], [93.8, 335523.0], [93.9, 335523.0], [94.0, 335523.0], [94.1, 335523.0], [94.2, 335523.0], [94.3, 336021.0], [94.4, 336021.0], [94.5, 336021.0], [94.6, 336021.0], [94.7, 336516.0], [94.8, 336516.0], [94.9, 336516.0], [95.0, 336516.0], [95.1, 336516.0], [95.2, 337017.0], [95.3, 337017.0], [95.4, 337017.0], [95.5, 337017.0], [95.6, 337515.0], [95.7, 337515.0], [95.8, 337515.0], [95.9, 337515.0], [96.0, 338012.0], [96.1, 338012.0], [96.2, 338012.0], [96.3, 338012.0], [96.4, 338012.0], [96.5, 338511.0], [96.6, 338511.0], [96.7, 338511.0], [96.8, 338511.0], [96.9, 339007.0], [97.0, 339007.0], [97.1, 339007.0], [97.2, 339007.0], [97.3, 339007.0], [97.4, 339507.0], [97.5, 339507.0], [97.6, 339507.0], [97.7, 339507.0], [97.8, 340002.0], [97.9, 340002.0], [98.0, 340002.0], [98.1, 340002.0], [98.2, 340002.0], [98.3, 340502.0], [98.4, 340502.0], [98.5, 340502.0], [98.6, 340502.0], [98.7, 341000.0], [98.8, 341000.0], [98.9, 341000.0], [99.0, 341000.0], [99.1, 341000.0], [99.2, 341496.0], [99.3, 341496.0], [99.4, 341496.0], [99.5, 341496.0], [99.6, 341997.0], [99.7, 341997.0], [99.8, 341997.0], [99.9, 341997.0]], "isOverall": false, "label": "AsignarDronPlan", "isController": false}], "supportsControllersDiscrimination": true, "maxX": 100.0, "title": "Response Time Percentiles"}},
        getOptions: function() {
            return {
                series: {
                    points: { show: false }
                },
                legend: {
                    noColumns: 2,
                    show: true,
                    container: '#legendResponseTimePercentiles'
                },
                xaxis: {
                    tickDecimals: 1,
                    axisLabel: "Percentiles",
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20,
                },
                yaxis: {
                    axisLabel: "Percentile value in ms",
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20
                },
                grid: {
                    hoverable: true // IMPORTANT! this is needed for tooltip to
                                    // work
                },
                tooltip: true,
                tooltipOpts: {
                    content: "%s : %x.2 percentile was %y ms"
                },
                selection: { mode: "xy" },
            };
        },
        createGraph: function() {
            var data = this.data;
            var dataset = prepareData(data.result.series, $("#choicesResponseTimePercentiles"));
            var options = this.getOptions();
            prepareOptions(options, data);
            $.plot($("#flotResponseTimesPercentiles"), dataset, options);
            // setup overview
            $.plot($("#overviewResponseTimesPercentiles"), dataset, prepareOverviewOptions(options));
        }
};

/**
 * @param elementId Id of element where we display message
 */
function setEmptyGraph(elementId) {
    $(function() {
        $(elementId).text("No graph series with filter="+seriesFilter);
    });
}

// Response times percentiles
function refreshResponseTimePercentiles() {
    var infos = responseTimePercentilesInfos;
    prepareSeries(infos.data);
    if(infos.data.result.series.length == 0) {
        setEmptyGraph("#bodyResponseTimePercentiles");
        return;
    }
    if (isGraph($("#flotResponseTimesPercentiles"))){
        infos.createGraph();
    } else {
        var choiceContainer = $("#choicesResponseTimePercentiles");
        createLegend(choiceContainer, infos);
        infos.createGraph();
        setGraphZoomable("#flotResponseTimesPercentiles", "#overviewResponseTimesPercentiles");
        $('#bodyResponseTimePercentiles .legendColorBox > div').each(function(i){
            $(this).clone().prependTo(choiceContainer.find("li").eq(i));
        });
    }
}

var responseTimeDistributionInfos = {
        data: {"result": {"minY": 1.0, "minX": 0.0, "maxY": 91.0, "series": [{"data": [[0.0, 91.0], [293700.0, 1.0], [285700.0, 1.0], [308100.0, 1.0], [301700.0, 1.0], [314500.0, 1.0], [322500.0, 1.0], [330500.0, 1.0], [338500.0, 1.0], [291200.0, 1.0], [283200.0, 1.0], [305600.0, 1.0], [299200.0, 1.0], [320000.0, 1.0], [328000.0, 1.0], [336000.0, 1.0], [290700.0, 1.0], [305100.0, 1.0], [298700.0, 1.0], [313100.0, 1.0], [319500.0, 1.0], [327500.0, 1.0], [335500.0, 1.0], [341900.0, 1.0], [288200.0, 1.0], [310600.0, 1.0], [296200.0, 1.0], [325000.0, 1.0], [317000.0, 1.0], [333000.0, 1.0], [341000.0, 1.0], [287700.0, 1.0], [310100.0, 1.0], [295700.0, 1.0], [303700.0, 1.0], [316500.0, 1.0], [324500.0, 1.0], [332500.0, 1.0], [340500.0, 1.0], [293200.0, 1.0], [285200.0, 1.0], [307600.0, 1.0], [301200.0, 1.0], [314000.0, 1.0], [322000.0, 1.0], [330000.0, 1.0], [338000.0, 1.0], [100.0, 11.0], [292700.0, 1.0], [284700.0, 1.0], [307100.0, 1.0], [300700.0, 1.0], [321500.0, 1.0], [313500.0, 1.0], [329500.0, 1.0], [337500.0, 1.0], [200.0, 1.0], [290200.0, 1.0], [304600.0, 1.0], [298200.0, 1.0], [319000.0, 1.0], [327000.0, 1.0], [335000.0, 1.0], [341400.0, 1.0], [500.0, 2.0], [289700.0, 1.0], [304100.0, 1.0], [297700.0, 1.0], [312100.0, 1.0], [318500.0, 1.0], [326500.0, 1.0], [334500.0, 1.0], [1000.0, 1.0], [287200.0, 1.0], [309600.0, 1.0], [303200.0, 1.0], [295200.0, 1.0], [324000.0, 1.0], [316000.0, 1.0], [332000.0, 1.0], [340000.0, 1.0], [294700.0, 1.0], [286700.0, 1.0], [309100.0, 1.0], [302700.0, 1.0], [315500.0, 1.0], [323500.0, 1.0], [331500.0, 1.0], [339500.0, 1.0], [292200.0, 1.0], [284200.0, 1.0], [306600.0, 1.0], [300200.0, 1.0], [321000.0, 1.0], [329000.0, 1.0], [337000.0, 1.0], [291700.0, 1.0], [283700.0, 1.0], [306100.0, 1.0], [299700.0, 1.0], [312500.0, 1.0], [320500.0, 1.0], [328500.0, 1.0], [336500.0, 1.0], [289200.0, 1.0], [297200.0, 1.0], [318000.0, 1.0], [311600.0, 1.0], [326000.0, 1.0], [334000.0, 1.0], [288700.0, 1.0], [311100.0, 1.0], [296700.0, 1.0], [317500.0, 1.0], [325500.0, 1.0], [333500.0, 1.0], [294200.0, 1.0], [286200.0, 1.0], [308600.0, 1.0], [302200.0, 1.0], [323000.0, 1.0], [315000.0, 1.0], [331000.0, 1.0], [339000.0, 1.0]], "isOverall": false, "label": "AsignarDronPlan", "isController": false}], "supportsControllersDiscrimination": true, "granularity": 100, "maxX": 341900.0, "title": "Response Time Distribution"}},
        getOptions: function() {
            var granularity = this.data.result.granularity;
            return {
                legend: {
                    noColumns: 2,
                    show: true,
                    container: '#legendResponseTimeDistribution'
                },
                xaxis:{
                    axisLabel: "Response times in ms",
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20,
                },
                yaxis: {
                    axisLabel: "Number of responses",
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20,
                },
                bars : {
                    show: true,
                    barWidth: this.data.result.granularity
                },
                grid: {
                    hoverable: true // IMPORTANT! this is needed for tooltip to
                                    // work
                },
                tooltip: true,
                tooltipOpts: {
                    content: function(label, xval, yval, flotItem){
                        return yval + " responses for " + label + " were between " + xval + " and " + (xval + granularity) + " ms";
                    }
                }
            };
        },
        createGraph: function() {
            var data = this.data;
            var options = this.getOptions();
            prepareOptions(options, data);
            $.plot($("#flotResponseTimeDistribution"), prepareData(data.result.series, $("#choicesResponseTimeDistribution")), options);
        }

};

// Response time distribution
function refreshResponseTimeDistribution() {
    var infos = responseTimeDistributionInfos;
    prepareSeries(infos.data);
    if(infos.data.result.series.length == 0) {
        setEmptyGraph("#bodyResponseTimeDistribution");
        return;
    }
    if (isGraph($("#flotResponseTimeDistribution"))){
        infos.createGraph();
    }else{
        var choiceContainer = $("#choicesResponseTimeDistribution");
        createLegend(choiceContainer, infos);
        infos.createGraph();
        $('#footerResponseTimeDistribution .legendColorBox > div').each(function(i){
            $(this).clone().prependTo(choiceContainer.find("li").eq(i));
        });
    }
};


var syntheticResponseTimeDistributionInfos = {
        data: {"result": {"minY": 3.0, "minX": 0.0, "ticks": [[0, "Requests having \nresponse time <= 500ms"], [1, "Requests having \nresponse time > 500ms and <= 1,500ms"], [2, "Requests having \nresponse time > 1,500ms"], [3, "Requests in error"]], "maxY": 119.0, "series": [{"data": [[0.0, 103.0]], "color": "#9ACD32", "isOverall": false, "label": "Requests having \nresponse time <= 500ms", "isController": false}, {"data": [[1.0, 3.0]], "color": "yellow", "isOverall": false, "label": "Requests having \nresponse time > 500ms and <= 1,500ms", "isController": false}, {"data": [], "color": "orange", "isOverall": false, "label": "Requests having \nresponse time > 1,500ms", "isController": false}, {"data": [[3.0, 119.0]], "color": "#FF6347", "isOverall": false, "label": "Requests in error", "isController": false}], "supportsControllersDiscrimination": false, "maxX": 3.0, "title": "Synthetic Response Times Distribution"}},
        getOptions: function() {
            return {
                legend: {
                    noColumns: 2,
                    show: true,
                    container: '#legendSyntheticResponseTimeDistribution'
                },
                xaxis:{
                    axisLabel: "Response times ranges",
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20,
                    tickLength:0,
                    min:-0.5,
                    max:3.5
                },
                yaxis: {
                    axisLabel: "Number of responses",
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20,
                },
                bars : {
                    show: true,
                    align: "center",
                    barWidth: 0.25,
                    fill:.75
                },
                grid: {
                    hoverable: true // IMPORTANT! this is needed for tooltip to
                                    // work
                },
                tooltip: true,
                tooltipOpts: {
                    content: function(label, xval, yval, flotItem){
                        return yval + " " + label;
                    }
                }
            };
        },
        createGraph: function() {
            var data = this.data;
            var options = this.getOptions();
            prepareOptions(options, data);
            options.xaxis.ticks = data.result.ticks;
            $.plot($("#flotSyntheticResponseTimeDistribution"), prepareData(data.result.series, $("#choicesSyntheticResponseTimeDistribution")), options);
        }

};

// Response time distribution
function refreshSyntheticResponseTimeDistribution() {
    var infos = syntheticResponseTimeDistributionInfos;
    prepareSeries(infos.data, true);
    if (isGraph($("#flotSyntheticResponseTimeDistribution"))){
        infos.createGraph();
    }else{
        var choiceContainer = $("#choicesSyntheticResponseTimeDistribution");
        createLegend(choiceContainer, infos);
        infos.createGraph();
        $('#footerSyntheticResponseTimeDistribution .legendColorBox > div').each(function(i){
            $(this).clone().prependTo(choiceContainer.find("li").eq(i));
        });
    }
};

var activeThreadsOverTimeInfos = {
        data: {"result": {"minY": 1.0348837209302324, "minX": 1.77210438E12, "maxY": 75.5042016806723, "series": [{"data": [[1.7721048E12, 75.5042016806723], [1.77210438E12, 1.1000000000000003], [1.77210444E12, 1.0348837209302324]], "isOverall": false, "label": "Grosso modo", "isController": false}], "supportsControllersDiscrimination": false, "granularity": 60000, "maxX": 1.7721048E12, "title": "Active Threads Over Time"}},
        getOptions: function() {
            return {
                series: {
                    stack: true,
                    lines: {
                        show: true,
                        fill: true
                    },
                    points: {
                        show: true
                    }
                },
                xaxis: {
                    mode: "time",
                    timeformat: getTimeFormat(this.data.result.granularity),
                    axisLabel: getElapsedTimeLabel(this.data.result.granularity),
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20,
                },
                yaxis: {
                    axisLabel: "Number of active threads",
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20
                },
                legend: {
                    noColumns: 6,
                    show: true,
                    container: '#legendActiveThreadsOverTime'
                },
                grid: {
                    hoverable: true // IMPORTANT! this is needed for tooltip to
                                    // work
                },
                selection: {
                    mode: 'xy'
                },
                tooltip: true,
                tooltipOpts: {
                    content: "%s : At %x there were %y active threads"
                }
            };
        },
        createGraph: function() {
            var data = this.data;
            var dataset = prepareData(data.result.series, $("#choicesActiveThreadsOverTime"));
            var options = this.getOptions();
            prepareOptions(options, data);
            $.plot($("#flotActiveThreadsOverTime"), dataset, options);
            // setup overview
            $.plot($("#overviewActiveThreadsOverTime"), dataset, prepareOverviewOptions(options));
        }
};

// Active Threads Over Time
function refreshActiveThreadsOverTime(fixTimestamps) {
    var infos = activeThreadsOverTimeInfos;
    prepareSeries(infos.data);
    if(fixTimestamps) {
        fixTimeStamps(infos.data.result.series, 3600000);
    }
    if(isGraph($("#flotActiveThreadsOverTime"))) {
        infos.createGraph();
    }else{
        var choiceContainer = $("#choicesActiveThreadsOverTime");
        createLegend(choiceContainer, infos);
        infos.createGraph();
        setGraphZoomable("#flotActiveThreadsOverTime", "#overviewActiveThreadsOverTime");
        $('#footerActiveThreadsOverTime .legendColorBox > div').each(function(i){
            $(this).clone().prependTo(choiceContainer.find("li").eq(i));
        });
    }
};

var timeVsThreadsInfos = {
        data: {"result": {"minY": 29.99019607843137, "minX": 1.0, "maxY": 341997.0, "series": [{"data": [[2.0, 588.3333333333334], [39.0, 289261.0], [41.0, 289758.0], [42.0, 293691.375], [3.0, 533.0], [54.0, 341496.0], [58.0, 341997.0], [61.0, 338925.1666666666], [60.0, 335026.0], [63.0, 338012.0], [1.0, 29.99019607843137], [66.0, 333275.5], [65.0, 336518.6666666667], [64.0, 337266.0], [71.0, 330037.25000000006], [68.0, 333655.0], [76.0, 326547.6], [114.0, 316921.6538461539], [112.0, 317909.6666666667], [113.0, 318323.5], [119.0, 307248.375], [118.0, 308618.0], [117.0, 310858.5]], "isOverall": false, "label": "AsignarDronPlan", "isController": false}, {"data": [[40.422222222222196, 165367.49777777772]], "isOverall": false, "label": "AsignarDronPlan-Aggregated", "isController": false}], "supportsControllersDiscrimination": true, "maxX": 119.0, "title": "Time VS Threads"}},
        getOptions: function() {
            return {
                series: {
                    lines: {
                        show: true
                    },
                    points: {
                        show: true
                    }
                },
                xaxis: {
                    axisLabel: "Number of active threads",
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20,
                },
                yaxis: {
                    axisLabel: "Average response times in ms",
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20
                },
                legend: { noColumns: 2,show: true, container: '#legendTimeVsThreads' },
                selection: {
                    mode: 'xy'
                },
                grid: {
                    hoverable: true // IMPORTANT! this is needed for tooltip to work
                },
                tooltip: true,
                tooltipOpts: {
                    content: "%s: At %x.2 active threads, Average response time was %y.2 ms"
                }
            };
        },
        createGraph: function() {
            var data = this.data;
            var dataset = prepareData(data.result.series, $("#choicesTimeVsThreads"));
            var options = this.getOptions();
            prepareOptions(options, data);
            $.plot($("#flotTimesVsThreads"), dataset, options);
            // setup overview
            $.plot($("#overviewTimesVsThreads"), dataset, prepareOverviewOptions(options));
        }
};

// Time vs threads
function refreshTimeVsThreads(){
    var infos = timeVsThreadsInfos;
    prepareSeries(infos.data);
    if(infos.data.result.series.length == 0) {
        setEmptyGraph("#bodyTimeVsThreads");
        return;
    }
    if(isGraph($("#flotTimesVsThreads"))){
        infos.createGraph();
    }else{
        var choiceContainer = $("#choicesTimeVsThreads");
        createLegend(choiceContainer, infos);
        infos.createGraph();
        setGraphZoomable("#flotTimesVsThreads", "#overviewTimesVsThreads");
        $('#footerTimeVsThreads .legendColorBox > div').each(function(i){
            $(this).clone().prependTo(choiceContainer.find("li").eq(i));
        });
    }
};

var bytesThroughputOverTimeInfos = {
        data : {"result": {"minY": 0.0, "minX": 1.77210438E12, "maxY": 4390.383333333333, "series": [{"data": [[1.7721048E12, 4390.383333333333], [1.77210438E12, 320.8666666666667], [1.77210444E12, 1382.25]], "isOverall": false, "label": "Bytes received per second", "isController": false}, {"data": [[1.7721048E12, 0.0], [1.77210438E12, 74.36666666666666], [1.77210444E12, 321.3]], "isOverall": false, "label": "Bytes sent per second", "isController": false}], "supportsControllersDiscrimination": false, "granularity": 60000, "maxX": 1.7721048E12, "title": "Bytes Throughput Over Time"}},
        getOptions : function(){
            return {
                series: {
                    lines: {
                        show: true
                    },
                    points: {
                        show: true
                    }
                },
                xaxis: {
                    mode: "time",
                    timeformat: getTimeFormat(this.data.result.granularity),
                    axisLabel: getElapsedTimeLabel(this.data.result.granularity) ,
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20,
                },
                yaxis: {
                    axisLabel: "Bytes / sec",
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20,
                },
                legend: {
                    noColumns: 2,
                    show: true,
                    container: '#legendBytesThroughputOverTime'
                },
                selection: {
                    mode: "xy"
                },
                grid: {
                    hoverable: true // IMPORTANT! this is needed for tooltip to
                                    // work
                },
                tooltip: true,
                tooltipOpts: {
                    content: "%s at %x was %y"
                }
            };
        },
        createGraph : function() {
            var data = this.data;
            var dataset = prepareData(data.result.series, $("#choicesBytesThroughputOverTime"));
            var options = this.getOptions();
            prepareOptions(options, data);
            $.plot($("#flotBytesThroughputOverTime"), dataset, options);
            // setup overview
            $.plot($("#overviewBytesThroughputOverTime"), dataset, prepareOverviewOptions(options));
        }
};

// Bytes throughput Over Time
function refreshBytesThroughputOverTime(fixTimestamps) {
    var infos = bytesThroughputOverTimeInfos;
    prepareSeries(infos.data);
    if(fixTimestamps) {
        fixTimeStamps(infos.data.result.series, 3600000);
    }
    if(isGraph($("#flotBytesThroughputOverTime"))){
        infos.createGraph();
    }else{
        var choiceContainer = $("#choicesBytesThroughputOverTime");
        createLegend(choiceContainer, infos);
        infos.createGraph();
        setGraphZoomable("#flotBytesThroughputOverTime", "#overviewBytesThroughputOverTime");
        $('#footerBytesThroughputOverTime .legendColorBox > div').each(function(i){
            $(this).clone().prependTo(choiceContainer.find("li").eq(i));
        });
    }
}

var responseTimesOverTimeInfos = {
        data: {"result": {"minY": 48.199999999999996, "minX": 1.77210438E12, "maxY": 312624.62184873933, "series": [{"data": [[1.7721048E12, 312624.62184873933], [1.77210438E12, 48.199999999999996], [1.77210444E12, 51.081395348837205]], "isOverall": false, "label": "AsignarDronPlan", "isController": false}], "supportsControllersDiscrimination": true, "granularity": 60000, "maxX": 1.7721048E12, "title": "Response Time Over Time"}},
        getOptions: function(){
            return {
                series: {
                    lines: {
                        show: true
                    },
                    points: {
                        show: true
                    }
                },
                xaxis: {
                    mode: "time",
                    timeformat: getTimeFormat(this.data.result.granularity),
                    axisLabel: getElapsedTimeLabel(this.data.result.granularity),
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20,
                },
                yaxis: {
                    axisLabel: "Average response time in ms",
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20,
                },
                legend: {
                    noColumns: 2,
                    show: true,
                    container: '#legendResponseTimesOverTime'
                },
                selection: {
                    mode: 'xy'
                },
                grid: {
                    hoverable: true // IMPORTANT! this is needed for tooltip to
                                    // work
                },
                tooltip: true,
                tooltipOpts: {
                    content: "%s : at %x Average response time was %y ms"
                }
            };
        },
        createGraph: function() {
            var data = this.data;
            var dataset = prepareData(data.result.series, $("#choicesResponseTimesOverTime"));
            var options = this.getOptions();
            prepareOptions(options, data);
            $.plot($("#flotResponseTimesOverTime"), dataset, options);
            // setup overview
            $.plot($("#overviewResponseTimesOverTime"), dataset, prepareOverviewOptions(options));
        }
};

// Response Times Over Time
function refreshResponseTimeOverTime(fixTimestamps) {
    var infos = responseTimesOverTimeInfos;
    prepareSeries(infos.data);
    if(infos.data.result.series.length == 0) {
        setEmptyGraph("#bodyResponseTimeOverTime");
        return;
    }
    if(fixTimestamps) {
        fixTimeStamps(infos.data.result.series, 3600000);
    }
    if(isGraph($("#flotResponseTimesOverTime"))){
        infos.createGraph();
    }else{
        var choiceContainer = $("#choicesResponseTimesOverTime");
        createLegend(choiceContainer, infos);
        infos.createGraph();
        setGraphZoomable("#flotResponseTimesOverTime", "#overviewResponseTimesOverTime");
        $('#footerResponseTimesOverTime .legendColorBox > div').each(function(i){
            $(this).clone().prependTo(choiceContainer.find("li").eq(i));
        });
    }
};

var latenciesOverTimeInfos = {
        data: {"result": {"minY": 0.0, "minX": 1.77210438E12, "maxY": 50.94186046511627, "series": [{"data": [[1.7721048E12, 0.0], [1.77210438E12, 48.15], [1.77210444E12, 50.94186046511627]], "isOverall": false, "label": "AsignarDronPlan", "isController": false}], "supportsControllersDiscrimination": true, "granularity": 60000, "maxX": 1.7721048E12, "title": "Latencies Over Time"}},
        getOptions: function() {
            return {
                series: {
                    lines: {
                        show: true
                    },
                    points: {
                        show: true
                    }
                },
                xaxis: {
                    mode: "time",
                    timeformat: getTimeFormat(this.data.result.granularity),
                    axisLabel: getElapsedTimeLabel(this.data.result.granularity),
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20,
                },
                yaxis: {
                    axisLabel: "Average response latencies in ms",
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20,
                },
                legend: {
                    noColumns: 2,
                    show: true,
                    container: '#legendLatenciesOverTime'
                },
                selection: {
                    mode: 'xy'
                },
                grid: {
                    hoverable: true // IMPORTANT! this is needed for tooltip to
                                    // work
                },
                tooltip: true,
                tooltipOpts: {
                    content: "%s : at %x Average latency was %y ms"
                }
            };
        },
        createGraph: function () {
            var data = this.data;
            var dataset = prepareData(data.result.series, $("#choicesLatenciesOverTime"));
            var options = this.getOptions();
            prepareOptions(options, data);
            $.plot($("#flotLatenciesOverTime"), dataset, options);
            // setup overview
            $.plot($("#overviewLatenciesOverTime"), dataset, prepareOverviewOptions(options));
        }
};

// Latencies Over Time
function refreshLatenciesOverTime(fixTimestamps) {
    var infos = latenciesOverTimeInfos;
    prepareSeries(infos.data);
    if(infos.data.result.series.length == 0) {
        setEmptyGraph("#bodyLatenciesOverTime");
        return;
    }
    if(fixTimestamps) {
        fixTimeStamps(infos.data.result.series, 3600000);
    }
    if(isGraph($("#flotLatenciesOverTime"))) {
        infos.createGraph();
    }else {
        var choiceContainer = $("#choicesLatenciesOverTime");
        createLegend(choiceContainer, infos);
        infos.createGraph();
        setGraphZoomable("#flotLatenciesOverTime", "#overviewLatenciesOverTime");
        $('#footerLatenciesOverTime .legendColorBox > div').each(function(i){
            $(this).clone().prependTo(choiceContainer.find("li").eq(i));
        });
    }
};

var connectTimeOverTimeInfos = {
        data: {"result": {"minY": 1.8319327731092434, "minX": 1.77210438E12, "maxY": 2.5500000000000003, "series": [{"data": [[1.7721048E12, 1.8319327731092434], [1.77210438E12, 2.5500000000000003], [1.77210444E12, 1.8372093023255813]], "isOverall": false, "label": "AsignarDronPlan", "isController": false}], "supportsControllersDiscrimination": true, "granularity": 60000, "maxX": 1.7721048E12, "title": "Connect Time Over Time"}},
        getOptions: function() {
            return {
                series: {
                    lines: {
                        show: true
                    },
                    points: {
                        show: true
                    }
                },
                xaxis: {
                    mode: "time",
                    timeformat: getTimeFormat(this.data.result.granularity),
                    axisLabel: getConnectTimeLabel(this.data.result.granularity),
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20,
                },
                yaxis: {
                    axisLabel: "Average Connect Time in ms",
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20,
                },
                legend: {
                    noColumns: 2,
                    show: true,
                    container: '#legendConnectTimeOverTime'
                },
                selection: {
                    mode: 'xy'
                },
                grid: {
                    hoverable: true // IMPORTANT! this is needed for tooltip to
                                    // work
                },
                tooltip: true,
                tooltipOpts: {
                    content: "%s : at %x Average connect time was %y ms"
                }
            };
        },
        createGraph: function () {
            var data = this.data;
            var dataset = prepareData(data.result.series, $("#choicesConnectTimeOverTime"));
            var options = this.getOptions();
            prepareOptions(options, data);
            $.plot($("#flotConnectTimeOverTime"), dataset, options);
            // setup overview
            $.plot($("#overviewConnectTimeOverTime"), dataset, prepareOverviewOptions(options));
        }
};

// Connect Time Over Time
function refreshConnectTimeOverTime(fixTimestamps) {
    var infos = connectTimeOverTimeInfos;
    prepareSeries(infos.data);
    if(infos.data.result.series.length == 0) {
        setEmptyGraph("#bodyConnectTimeOverTime");
        return;
    }
    if(fixTimestamps) {
        fixTimeStamps(infos.data.result.series, 3600000);
    }
    if(isGraph($("#flotConnectTimeOverTime"))) {
        infos.createGraph();
    }else {
        var choiceContainer = $("#choicesConnectTimeOverTime");
        createLegend(choiceContainer, infos);
        infos.createGraph();
        setGraphZoomable("#flotConnectTimeOverTime", "#overviewConnectTimeOverTime");
        $('#footerConnectTimeOverTime .legendColorBox > div').each(function(i){
            $(this).clone().prependTo(choiceContainer.find("li").eq(i));
        });
    }
};

var responseTimePercentilesOverTimeInfos = {
        data: {"result": {"minY": 6.0, "minX": 1.77210438E12, "maxY": 1061.0, "series": [{"data": [[1.77210438E12, 574.0], [1.77210444E12, 1061.0]], "isOverall": false, "label": "Max", "isController": false}, {"data": [[1.77210438E12, 119.50000000000023], [1.77210444E12, 160.3]], "isOverall": false, "label": "90th percentile", "isController": false}, {"data": [[1.77210438E12, 574.0], [1.77210444E12, 1061.0]], "isOverall": false, "label": "99th percentile", "isController": false}, {"data": [[1.77210438E12, 551.7999999999997], [1.77210444E12, 165.89999999999995]], "isOverall": false, "label": "95th percentile", "isController": false}, {"data": [[1.77210438E12, 10.0], [1.77210444E12, 6.0]], "isOverall": false, "label": "Min", "isController": false}, {"data": [[1.77210438E12, 14.5], [1.77210444E12, 11.5]], "isOverall": false, "label": "Median", "isController": false}], "supportsControllersDiscrimination": false, "granularity": 60000, "maxX": 1.77210444E12, "title": "Response Time Percentiles Over Time (successful requests only)"}},
        getOptions: function() {
            return {
                series: {
                    lines: {
                        show: true,
                        fill: true
                    },
                    points: {
                        show: true
                    }
                },
                xaxis: {
                    mode: "time",
                    timeformat: getTimeFormat(this.data.result.granularity),
                    axisLabel: getElapsedTimeLabel(this.data.result.granularity),
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20,
                },
                yaxis: {
                    axisLabel: "Response Time in ms",
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20,
                },
                legend: {
                    noColumns: 2,
                    show: true,
                    container: '#legendResponseTimePercentilesOverTime'
                },
                selection: {
                    mode: 'xy'
                },
                grid: {
                    hoverable: true // IMPORTANT! this is needed for tooltip to
                                    // work
                },
                tooltip: true,
                tooltipOpts: {
                    content: "%s : at %x Response time was %y ms"
                }
            };
        },
        createGraph: function () {
            var data = this.data;
            var dataset = prepareData(data.result.series, $("#choicesResponseTimePercentilesOverTime"));
            var options = this.getOptions();
            prepareOptions(options, data);
            $.plot($("#flotResponseTimePercentilesOverTime"), dataset, options);
            // setup overview
            $.plot($("#overviewResponseTimePercentilesOverTime"), dataset, prepareOverviewOptions(options));
        }
};

// Response Time Percentiles Over Time
function refreshResponseTimePercentilesOverTime(fixTimestamps) {
    var infos = responseTimePercentilesOverTimeInfos;
    prepareSeries(infos.data);
    if(fixTimestamps) {
        fixTimeStamps(infos.data.result.series, 3600000);
    }
    if(isGraph($("#flotResponseTimePercentilesOverTime"))) {
        infos.createGraph();
    }else {
        var choiceContainer = $("#choicesResponseTimePercentilesOverTime");
        createLegend(choiceContainer, infos);
        infos.createGraph();
        setGraphZoomable("#flotResponseTimePercentilesOverTime", "#overviewResponseTimePercentilesOverTime");
        $('#footerResponseTimePercentilesOverTime .legendColorBox > div').each(function(i){
            $(this).clone().prependTo(choiceContainer.find("li").eq(i));
        });
    }
};


var responseTimeVsRequestInfos = {
    data: {"result": {"minY": 12.0, "minX": 1.0, "maxY": 312588.0, "series": [{"data": [[2.0, 12.0], [1.0, 84.0], [3.0, 115.5]], "isOverall": false, "label": "Successes", "isController": false}, {"data": [[119.0, 312588.0]], "isOverall": false, "label": "Failures", "isController": false}], "supportsControllersDiscrimination": false, "granularity": 1000, "maxX": 119.0, "title": "Response Time Vs Request"}},
    getOptions: function() {
        return {
            series: {
                lines: {
                    show: false
                },
                points: {
                    show: true
                }
            },
            xaxis: {
                axisLabel: "Global number of requests per second",
                axisLabelUseCanvas: true,
                axisLabelFontSizePixels: 12,
                axisLabelFontFamily: 'Verdana, Arial',
                axisLabelPadding: 20,
            },
            yaxis: {
                axisLabel: "Median Response Time in ms",
                axisLabelUseCanvas: true,
                axisLabelFontSizePixels: 12,
                axisLabelFontFamily: 'Verdana, Arial',
                axisLabelPadding: 20,
            },
            legend: {
                noColumns: 2,
                show: true,
                container: '#legendResponseTimeVsRequest'
            },
            selection: {
                mode: 'xy'
            },
            grid: {
                hoverable: true // IMPORTANT! this is needed for tooltip to work
            },
            tooltip: true,
            tooltipOpts: {
                content: "%s : Median response time at %x req/s was %y ms"
            },
            colors: ["#9ACD32", "#FF6347"]
        };
    },
    createGraph: function () {
        var data = this.data;
        var dataset = prepareData(data.result.series, $("#choicesResponseTimeVsRequest"));
        var options = this.getOptions();
        prepareOptions(options, data);
        $.plot($("#flotResponseTimeVsRequest"), dataset, options);
        // setup overview
        $.plot($("#overviewResponseTimeVsRequest"), dataset, prepareOverviewOptions(options));

    }
};

// Response Time vs Request
function refreshResponseTimeVsRequest() {
    var infos = responseTimeVsRequestInfos;
    prepareSeries(infos.data);
    if (isGraph($("#flotResponseTimeVsRequest"))){
        infos.createGraph();
    }else{
        var choiceContainer = $("#choicesResponseTimeVsRequest");
        createLegend(choiceContainer, infos);
        infos.createGraph();
        setGraphZoomable("#flotResponseTimeVsRequest", "#overviewResponseTimeVsRequest");
        $('#footerResponseRimeVsRequest .legendColorBox > div').each(function(i){
            $(this).clone().prependTo(choiceContainer.find("li").eq(i));
        });
    }
};


var latenciesVsRequestInfos = {
    data: {"result": {"minY": 0.0, "minX": 1.0, "maxY": 115.5, "series": [{"data": [[2.0, 11.5], [1.0, 83.5], [3.0, 115.5]], "isOverall": false, "label": "Successes", "isController": false}, {"data": [[119.0, 0.0]], "isOverall": false, "label": "Failures", "isController": false}], "supportsControllersDiscrimination": false, "granularity": 1000, "maxX": 119.0, "title": "Latencies Vs Request"}},
    getOptions: function() {
        return{
            series: {
                lines: {
                    show: false
                },
                points: {
                    show: true
                }
            },
            xaxis: {
                axisLabel: "Global number of requests per second",
                axisLabelUseCanvas: true,
                axisLabelFontSizePixels: 12,
                axisLabelFontFamily: 'Verdana, Arial',
                axisLabelPadding: 20,
            },
            yaxis: {
                axisLabel: "Median Latency in ms",
                axisLabelUseCanvas: true,
                axisLabelFontSizePixels: 12,
                axisLabelFontFamily: 'Verdana, Arial',
                axisLabelPadding: 20,
            },
            legend: { noColumns: 2,show: true, container: '#legendLatencyVsRequest' },
            selection: {
                mode: 'xy'
            },
            grid: {
                hoverable: true // IMPORTANT! this is needed for tooltip to work
            },
            tooltip: true,
            tooltipOpts: {
                content: "%s : Median Latency time at %x req/s was %y ms"
            },
            colors: ["#9ACD32", "#FF6347"]
        };
    },
    createGraph: function () {
        var data = this.data;
        var dataset = prepareData(data.result.series, $("#choicesLatencyVsRequest"));
        var options = this.getOptions();
        prepareOptions(options, data);
        $.plot($("#flotLatenciesVsRequest"), dataset, options);
        // setup overview
        $.plot($("#overviewLatenciesVsRequest"), dataset, prepareOverviewOptions(options));
    }
};

// Latencies vs Request
function refreshLatenciesVsRequest() {
        var infos = latenciesVsRequestInfos;
        prepareSeries(infos.data);
        if(isGraph($("#flotLatenciesVsRequest"))){
            infos.createGraph();
        }else{
            var choiceContainer = $("#choicesLatencyVsRequest");
            createLegend(choiceContainer, infos);
            infos.createGraph();
            setGraphZoomable("#flotLatenciesVsRequest", "#overviewLatenciesVsRequest");
            $('#footerLatenciesVsRequest .legendColorBox > div').each(function(i){
                $(this).clone().prependTo(choiceContainer.find("li").eq(i));
            });
        }
};

var hitsPerSecondInfos = {
        data: {"result": {"minY": 0.3333333333333333, "minX": 1.77210438E12, "maxY": 2.0166666666666666, "series": [{"data": [[1.7721045E12, 1.4], [1.77210438E12, 0.3333333333333333], [1.77210444E12, 2.0166666666666666]], "isOverall": false, "label": "hitsPerSecond", "isController": false}], "supportsControllersDiscrimination": false, "granularity": 60000, "maxX": 1.7721045E12, "title": "Hits Per Second"}},
        getOptions: function() {
            return {
                series: {
                    lines: {
                        show: true
                    },
                    points: {
                        show: true
                    }
                },
                xaxis: {
                    mode: "time",
                    timeformat: getTimeFormat(this.data.result.granularity),
                    axisLabel: getElapsedTimeLabel(this.data.result.granularity),
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20,
                },
                yaxis: {
                    axisLabel: "Number of hits / sec",
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20
                },
                legend: {
                    noColumns: 2,
                    show: true,
                    container: "#legendHitsPerSecond"
                },
                selection: {
                    mode : 'xy'
                },
                grid: {
                    hoverable: true // IMPORTANT! this is needed for tooltip to
                                    // work
                },
                tooltip: true,
                tooltipOpts: {
                    content: "%s at %x was %y.2 hits/sec"
                }
            };
        },
        createGraph: function createGraph() {
            var data = this.data;
            var dataset = prepareData(data.result.series, $("#choicesHitsPerSecond"));
            var options = this.getOptions();
            prepareOptions(options, data);
            $.plot($("#flotHitsPerSecond"), dataset, options);
            // setup overview
            $.plot($("#overviewHitsPerSecond"), dataset, prepareOverviewOptions(options));
        }
};

// Hits per second
function refreshHitsPerSecond(fixTimestamps) {
    var infos = hitsPerSecondInfos;
    prepareSeries(infos.data);
    if(fixTimestamps) {
        fixTimeStamps(infos.data.result.series, 3600000);
    }
    if (isGraph($("#flotHitsPerSecond"))){
        infos.createGraph();
    }else{
        var choiceContainer = $("#choicesHitsPerSecond");
        createLegend(choiceContainer, infos);
        infos.createGraph();
        setGraphZoomable("#flotHitsPerSecond", "#overviewHitsPerSecond");
        $('#footerHitsPerSecond .legendColorBox > div').each(function(i){
            $(this).clone().prependTo(choiceContainer.find("li").eq(i));
        });
    }
}

var codesPerSecondInfos = {
        data: {"result": {"minY": 0.3333333333333333, "minX": 1.77210438E12, "maxY": 1.4333333333333333, "series": [{"data": [[1.77210438E12, 0.3333333333333333], [1.77210444E12, 1.4333333333333333]], "isOverall": false, "label": "200", "isController": false}, {"data": [[1.7721048E12, 1.2666666666666666]], "isOverall": false, "label": "Non HTTP response code: org.apache.http.NoHttpResponseException", "isController": false}, {"data": [[1.7721048E12, 0.7166666666666667]], "isOverall": false, "label": "Non HTTP response code: java.net.SocketException", "isController": false}], "supportsControllersDiscrimination": false, "granularity": 60000, "maxX": 1.7721048E12, "title": "Codes Per Second"}},
        getOptions: function(){
            return {
                series: {
                    lines: {
                        show: true
                    },
                    points: {
                        show: true
                    }
                },
                xaxis: {
                    mode: "time",
                    timeformat: getTimeFormat(this.data.result.granularity),
                    axisLabel: getElapsedTimeLabel(this.data.result.granularity),
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20,
                },
                yaxis: {
                    axisLabel: "Number of responses / sec",
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20,
                },
                legend: {
                    noColumns: 2,
                    show: true,
                    container: "#legendCodesPerSecond"
                },
                selection: {
                    mode: 'xy'
                },
                grid: {
                    hoverable: true // IMPORTANT! this is needed for tooltip to
                                    // work
                },
                tooltip: true,
                tooltipOpts: {
                    content: "Number of Response Codes %s at %x was %y.2 responses / sec"
                }
            };
        },
    createGraph: function() {
        var data = this.data;
        var dataset = prepareData(data.result.series, $("#choicesCodesPerSecond"));
        var options = this.getOptions();
        prepareOptions(options, data);
        $.plot($("#flotCodesPerSecond"), dataset, options);
        // setup overview
        $.plot($("#overviewCodesPerSecond"), dataset, prepareOverviewOptions(options));
    }
};

// Codes per second
function refreshCodesPerSecond(fixTimestamps) {
    var infos = codesPerSecondInfos;
    prepareSeries(infos.data);
    if(fixTimestamps) {
        fixTimeStamps(infos.data.result.series, 3600000);
    }
    if(isGraph($("#flotCodesPerSecond"))){
        infos.createGraph();
    }else{
        var choiceContainer = $("#choicesCodesPerSecond");
        createLegend(choiceContainer, infos);
        infos.createGraph();
        setGraphZoomable("#flotCodesPerSecond", "#overviewCodesPerSecond");
        $('#footerCodesPerSecond .legendColorBox > div').each(function(i){
            $(this).clone().prependTo(choiceContainer.find("li").eq(i));
        });
    }
};

var transactionsPerSecondInfos = {
        data: {"result": {"minY": 0.3333333333333333, "minX": 1.77210438E12, "maxY": 1.9833333333333334, "series": [{"data": [[1.77210438E12, 0.3333333333333333], [1.77210444E12, 1.4333333333333333]], "isOverall": false, "label": "AsignarDronPlan-success", "isController": false}, {"data": [[1.7721048E12, 1.9833333333333334]], "isOverall": false, "label": "AsignarDronPlan-failure", "isController": false}], "supportsControllersDiscrimination": true, "granularity": 60000, "maxX": 1.7721048E12, "title": "Transactions Per Second"}},
        getOptions: function(){
            return {
                series: {
                    lines: {
                        show: true
                    },
                    points: {
                        show: true
                    }
                },
                xaxis: {
                    mode: "time",
                    timeformat: getTimeFormat(this.data.result.granularity),
                    axisLabel: getElapsedTimeLabel(this.data.result.granularity),
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20,
                },
                yaxis: {
                    axisLabel: "Number of transactions / sec",
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20
                },
                legend: {
                    noColumns: 2,
                    show: true,
                    container: "#legendTransactionsPerSecond"
                },
                selection: {
                    mode: 'xy'
                },
                grid: {
                    hoverable: true // IMPORTANT! this is needed for tooltip to
                                    // work
                },
                tooltip: true,
                tooltipOpts: {
                    content: "%s at %x was %y transactions / sec"
                }
            };
        },
    createGraph: function () {
        var data = this.data;
        var dataset = prepareData(data.result.series, $("#choicesTransactionsPerSecond"));
        var options = this.getOptions();
        prepareOptions(options, data);
        $.plot($("#flotTransactionsPerSecond"), dataset, options);
        // setup overview
        $.plot($("#overviewTransactionsPerSecond"), dataset, prepareOverviewOptions(options));
    }
};

// Transactions per second
function refreshTransactionsPerSecond(fixTimestamps) {
    var infos = transactionsPerSecondInfos;
    prepareSeries(infos.data);
    if(infos.data.result.series.length == 0) {
        setEmptyGraph("#bodyTransactionsPerSecond");
        return;
    }
    if(fixTimestamps) {
        fixTimeStamps(infos.data.result.series, 3600000);
    }
    if(isGraph($("#flotTransactionsPerSecond"))){
        infos.createGraph();
    }else{
        var choiceContainer = $("#choicesTransactionsPerSecond");
        createLegend(choiceContainer, infos);
        infos.createGraph();
        setGraphZoomable("#flotTransactionsPerSecond", "#overviewTransactionsPerSecond");
        $('#footerTransactionsPerSecond .legendColorBox > div').each(function(i){
            $(this).clone().prependTo(choiceContainer.find("li").eq(i));
        });
    }
};

var totalTPSInfos = {
        data: {"result": {"minY": 0.3333333333333333, "minX": 1.77210438E12, "maxY": 1.9833333333333334, "series": [{"data": [[1.77210438E12, 0.3333333333333333], [1.77210444E12, 1.4333333333333333]], "isOverall": false, "label": "Transaction-success", "isController": false}, {"data": [[1.7721048E12, 1.9833333333333334]], "isOverall": false, "label": "Transaction-failure", "isController": false}], "supportsControllersDiscrimination": true, "granularity": 60000, "maxX": 1.7721048E12, "title": "Total Transactions Per Second"}},
        getOptions: function(){
            return {
                series: {
                    lines: {
                        show: true
                    },
                    points: {
                        show: true
                    }
                },
                xaxis: {
                    mode: "time",
                    timeformat: getTimeFormat(this.data.result.granularity),
                    axisLabel: getElapsedTimeLabel(this.data.result.granularity),
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20,
                },
                yaxis: {
                    axisLabel: "Number of transactions / sec",
                    axisLabelUseCanvas: true,
                    axisLabelFontSizePixels: 12,
                    axisLabelFontFamily: 'Verdana, Arial',
                    axisLabelPadding: 20
                },
                legend: {
                    noColumns: 2,
                    show: true,
                    container: "#legendTotalTPS"
                },
                selection: {
                    mode: 'xy'
                },
                grid: {
                    hoverable: true // IMPORTANT! this is needed for tooltip to
                                    // work
                },
                tooltip: true,
                tooltipOpts: {
                    content: "%s at %x was %y transactions / sec"
                },
                colors: ["#9ACD32", "#FF6347"]
            };
        },
    createGraph: function () {
        var data = this.data;
        var dataset = prepareData(data.result.series, $("#choicesTotalTPS"));
        var options = this.getOptions();
        prepareOptions(options, data);
        $.plot($("#flotTotalTPS"), dataset, options);
        // setup overview
        $.plot($("#overviewTotalTPS"), dataset, prepareOverviewOptions(options));
    }
};

// Total Transactions per second
function refreshTotalTPS(fixTimestamps) {
    var infos = totalTPSInfos;
    // We want to ignore seriesFilter
    prepareSeries(infos.data, false, true);
    if(fixTimestamps) {
        fixTimeStamps(infos.data.result.series, 3600000);
    }
    if(isGraph($("#flotTotalTPS"))){
        infos.createGraph();
    }else{
        var choiceContainer = $("#choicesTotalTPS");
        createLegend(choiceContainer, infos);
        infos.createGraph();
        setGraphZoomable("#flotTotalTPS", "#overviewTotalTPS");
        $('#footerTotalTPS .legendColorBox > div').each(function(i){
            $(this).clone().prependTo(choiceContainer.find("li").eq(i));
        });
    }
};

// Collapse the graph matching the specified DOM element depending the collapsed
// status
function collapse(elem, collapsed){
    if(collapsed){
        $(elem).parent().find(".fa-chevron-up").removeClass("fa-chevron-up").addClass("fa-chevron-down");
    } else {
        $(elem).parent().find(".fa-chevron-down").removeClass("fa-chevron-down").addClass("fa-chevron-up");
        if (elem.id == "bodyBytesThroughputOverTime") {
            if (isGraph($(elem).find('.flot-chart-content')) == false) {
                refreshBytesThroughputOverTime(true);
            }
            document.location.href="#bytesThroughputOverTime";
        } else if (elem.id == "bodyLatenciesOverTime") {
            if (isGraph($(elem).find('.flot-chart-content')) == false) {
                refreshLatenciesOverTime(true);
            }
            document.location.href="#latenciesOverTime";
        } else if (elem.id == "bodyCustomGraph") {
            if (isGraph($(elem).find('.flot-chart-content')) == false) {
                refreshCustomGraph(true);
            }
            document.location.href="#responseCustomGraph";
        } else if (elem.id == "bodyConnectTimeOverTime") {
            if (isGraph($(elem).find('.flot-chart-content')) == false) {
                refreshConnectTimeOverTime(true);
            }
            document.location.href="#connectTimeOverTime";
        } else if (elem.id == "bodyResponseTimePercentilesOverTime") {
            if (isGraph($(elem).find('.flot-chart-content')) == false) {
                refreshResponseTimePercentilesOverTime(true);
            }
            document.location.href="#responseTimePercentilesOverTime";
        } else if (elem.id == "bodyResponseTimeDistribution") {
            if (isGraph($(elem).find('.flot-chart-content')) == false) {
                refreshResponseTimeDistribution();
            }
            document.location.href="#responseTimeDistribution" ;
        } else if (elem.id == "bodySyntheticResponseTimeDistribution") {
            if (isGraph($(elem).find('.flot-chart-content')) == false) {
                refreshSyntheticResponseTimeDistribution();
            }
            document.location.href="#syntheticResponseTimeDistribution" ;
        } else if (elem.id == "bodyActiveThreadsOverTime") {
            if (isGraph($(elem).find('.flot-chart-content')) == false) {
                refreshActiveThreadsOverTime(true);
            }
            document.location.href="#activeThreadsOverTime";
        } else if (elem.id == "bodyTimeVsThreads") {
            if (isGraph($(elem).find('.flot-chart-content')) == false) {
                refreshTimeVsThreads();
            }
            document.location.href="#timeVsThreads" ;
        } else if (elem.id == "bodyCodesPerSecond") {
            if (isGraph($(elem).find('.flot-chart-content')) == false) {
                refreshCodesPerSecond(true);
            }
            document.location.href="#codesPerSecond";
        } else if (elem.id == "bodyTransactionsPerSecond") {
            if (isGraph($(elem).find('.flot-chart-content')) == false) {
                refreshTransactionsPerSecond(true);
            }
            document.location.href="#transactionsPerSecond";
        } else if (elem.id == "bodyTotalTPS") {
            if (isGraph($(elem).find('.flot-chart-content')) == false) {
                refreshTotalTPS(true);
            }
            document.location.href="#totalTPS";
        } else if (elem.id == "bodyResponseTimeVsRequest") {
            if (isGraph($(elem).find('.flot-chart-content')) == false) {
                refreshResponseTimeVsRequest();
            }
            document.location.href="#responseTimeVsRequest";
        } else if (elem.id == "bodyLatenciesVsRequest") {
            if (isGraph($(elem).find('.flot-chart-content')) == false) {
                refreshLatenciesVsRequest();
            }
            document.location.href="#latencyVsRequest";
        }
    }
}

/*
 * Activates or deactivates all series of the specified graph (represented by id parameter)
 * depending on checked argument.
 */
function toggleAll(id, checked){
    var placeholder = document.getElementById(id);

    var cases = $(placeholder).find(':checkbox');
    cases.prop('checked', checked);
    $(cases).parent().children().children().toggleClass("legend-disabled", !checked);

    var choiceContainer;
    if ( id == "choicesBytesThroughputOverTime"){
        choiceContainer = $("#choicesBytesThroughputOverTime");
        refreshBytesThroughputOverTime(false);
    } else if(id == "choicesResponseTimesOverTime"){
        choiceContainer = $("#choicesResponseTimesOverTime");
        refreshResponseTimeOverTime(false);
    }else if(id == "choicesResponseCustomGraph"){
        choiceContainer = $("#choicesResponseCustomGraph");
        refreshCustomGraph(false);
    } else if ( id == "choicesLatenciesOverTime"){
        choiceContainer = $("#choicesLatenciesOverTime");
        refreshLatenciesOverTime(false);
    } else if ( id == "choicesConnectTimeOverTime"){
        choiceContainer = $("#choicesConnectTimeOverTime");
        refreshConnectTimeOverTime(false);
    } else if ( id == "choicesResponseTimePercentilesOverTime"){
        choiceContainer = $("#choicesResponseTimePercentilesOverTime");
        refreshResponseTimePercentilesOverTime(false);
    } else if ( id == "choicesResponseTimePercentiles"){
        choiceContainer = $("#choicesResponseTimePercentiles");
        refreshResponseTimePercentiles();
    } else if(id == "choicesActiveThreadsOverTime"){
        choiceContainer = $("#choicesActiveThreadsOverTime");
        refreshActiveThreadsOverTime(false);
    } else if ( id == "choicesTimeVsThreads"){
        choiceContainer = $("#choicesTimeVsThreads");
        refreshTimeVsThreads();
    } else if ( id == "choicesSyntheticResponseTimeDistribution"){
        choiceContainer = $("#choicesSyntheticResponseTimeDistribution");
        refreshSyntheticResponseTimeDistribution();
    } else if ( id == "choicesResponseTimeDistribution"){
        choiceContainer = $("#choicesResponseTimeDistribution");
        refreshResponseTimeDistribution();
    } else if ( id == "choicesHitsPerSecond"){
        choiceContainer = $("#choicesHitsPerSecond");
        refreshHitsPerSecond(false);
    } else if(id == "choicesCodesPerSecond"){
        choiceContainer = $("#choicesCodesPerSecond");
        refreshCodesPerSecond(false);
    } else if ( id == "choicesTransactionsPerSecond"){
        choiceContainer = $("#choicesTransactionsPerSecond");
        refreshTransactionsPerSecond(false);
    } else if ( id == "choicesTotalTPS"){
        choiceContainer = $("#choicesTotalTPS");
        refreshTotalTPS(false);
    } else if ( id == "choicesResponseTimeVsRequest"){
        choiceContainer = $("#choicesResponseTimeVsRequest");
        refreshResponseTimeVsRequest();
    } else if ( id == "choicesLatencyVsRequest"){
        choiceContainer = $("#choicesLatencyVsRequest");
        refreshLatenciesVsRequest();
    }
    var color = checked ? "black" : "#818181";
    if(choiceContainer != null) {
        choiceContainer.find("label").each(function(){
            this.style.color = color;
        });
    }
}

