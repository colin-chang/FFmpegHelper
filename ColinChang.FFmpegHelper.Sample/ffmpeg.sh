#!/bin/bash

# use getopts to fix the problem that any parameters are null or empty result in disordered parameters issue.  
while getopts ":a:b:c:d:e:" opt
do
    case $opt in
        a)
        ffmpeg=$OPTARG
        ;;
        b)
        inputParameters=$OPTARG
        ;;
        c)
        input=$OPTARG
        ;;
        d)
        outputParameters=$OPTARG
        ;;
        e)
        output=$OPTARG
        ;;
        ?)
        echo "unknown parameter"
        exit 1;;
    esac
done

cd $ffmpeg

# make sure the output path must be an absolutely path.otherwise it will throw the "No such file or directory exception"
eval "./ffmpeg $inputParameters -i $input $outputParameters $output"


echo $?