import os, sys, getopt, cv2, shutil, PickleUtil

def Frames2Features(inputfolder, outputfolder, max_keypoints, filenameformat):
    # Remove older version of the processed video if it exists
    if (os.path.exists(outputfolder)):
        shutil.rmtree(outputfolder)
    os.makedirs(outputfolder)
    
    files = os.listdir(inputfolder)
    
    files_hashset = set(files)
    
    # Validate frames
    for i, file in enumerate(files):
        if (filenameformat.format(i) not in files_hashset):
            print("Invalid file found:", file, "expected:", filenameformat.format(i))
            sys.exit()

    # Generate features 
    sift = cv2.SIFT_create()
    
    total_files = len(files)
    
    current_percentage = 0;
    next_percentage = 0;
    stepsize = 0.05;
    
    for i, file in enumerate(files):
        img = cv2.imread(os.path.join(inputfolder, filenameformat.format(i)))
        gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
        
        tmpkeys, desc = sift.detectAndCompute(gray, None)
        
        largest_keypoints = sorted(tmpkeys, key=lambda point: point.size, reverse=True)[:max_keypoints]
        
        image=cv2.drawKeypoints(gray,largest_keypoints,img,flags=cv2.DRAW_MATCHES_FLAGS_DRAW_RICH_KEYPOINTS)
        
        keyp = [(point.pt, point.size, point.angle, point.response, point.octave, point.class_id, desc) for point in largest_keypoints]
        
        # Save the frame as a JPEG image file
        filename = os.path.join(outputfolder, file)
        cv2.imwrite(filename, image)
        
        PickleUtil.Save(os.path.join(outputfolder, file.replace(".jpg", ".keyp")), keyp)
        PickleUtil.Save(os.path.join(outputfolder, file.replace(".jpg", ".desc")), desc)
        
        current_percentage = (i+1) / total_files
        
        if (current_percentage > next_percentage):
            print(f"Processing frames {round(next_percentage, 2) * 100}%")
            next_percentage = next_percentage + stepsize

if __name__ == "__main__":
    inputfolder = ''
    outputfolder = ''
    max_keypoints = 100
    opts, args = getopt.getopt(sys.argv[1:], "hi:o:m:", ["input=", "output=", "max_keypoints="])
    for opt, arg in opts:
        if opt == '-h':
            print ('python ProcessFrames.py -i <inputfolder> -o <outputfolder> -m <max number of keypoints>')
            sys.exit()
        elif opt in ("-i"):
            inputfolder = arg
        elif opt in ("-o"):
            outputfolder = arg
        elif opt in ("-m"):
            max_keypoints = int(arg)
    print('Input folder is', inputfolder)
    print('Output folder is', outputfolder)
    print('Max number of keypoints is', max_keypoints)
    Frames2Features(inputfolder, outputfolder, max_keypoints, "frame{:0>5}.jpg")