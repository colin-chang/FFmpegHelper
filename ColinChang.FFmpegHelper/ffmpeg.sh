#!/bin/bash

cd $1

# make sure the output path must be an absolutely path.otherwise it will throw the "No such file or directory exception"

bi=$2
bi= ${bi//'$$'/''}
bo=$4
bo= ${bo//'$$'/''}

sc="./ffmpeg ${bi} -i $3 ${bo} $5"
sc=${sc//\'/\"}
echo ${sc}
eval ${sc}

echo $?