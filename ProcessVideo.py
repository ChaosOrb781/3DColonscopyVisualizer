import os, sys, getopt, cv2, shutil

def Video2Frames(inputfile, outputfolder, maskfile):
    # Remove older version of the processed video if it exists
    if (os.path.exists(outputfolder)):
        shutil.rmtree(outputfolder)
    os.makedirs(outputfolder)
    
    # Get the number of frames in the video
    video = cv2.VideoCapture(inputfile)
    num_frames = int(video.get(cv2.CAP_PROP_FRAME_COUNT))
    
    num_chars = len(str(num_frames))
    
    current_percentage = 0;
    next_percentage = 0;
    stepsize = 0.05;
    
    for i in range(num_frames):
        # Iterate through each frame and write it back to the output dir
        ret, frame = video.read()
    
        if ret:
            # Save the frame as a JPEG image file
            filename = os.path.join(outputfolder, "frame{:0>5}.jpg".format(i))
            cv2.imwrite(filename, frame)
        else:
            print(f"Could not read the image frame at {i} index")
        current_percentage = (i+1) / num_frames
        if (current_percentage > next_percentage):
            print(f"Processing video {round(next_percentage, 2) * 100}%")
            next_percentage = next_percentage + stepsize
    
    print(f"Processing {100.0}%")
    # Release the video file handle
    video.release()


if __name__ == "__main__":
    inputfile = ''
    outputfolder = ''
    maskfile = ''
    opts, args = getopt.getopt(sys.argv[1:], "hi:o:m:", ["input=", "output=", "mask="])
    for opt, arg in opts:
        if opt == '-h':
            print ('python ProcessVideo.py -i <inputfile> -o <outputfolder> -m <maskfile>')
            sys.exit()
        elif opt in ("-i"):
            inputfile = arg
        elif opt in ("-o"):
            outputfolder = arg
        elif opt in ("-m"):
            maskfile = arg
    print('Input file is', inputfile)
    print('Output folder is', outputfolder)
    print('Mask file is', maskfile)
    Video2Frames(inputfile, outputfolder)